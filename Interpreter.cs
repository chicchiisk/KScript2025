using Calculator.Runtime;

namespace Calculator;

/// <summary>
/// Main interpreter for the scripting language
/// </summary>
public class Interpreter : IExprVisitor<object?>, IStmtVisitor<object?>
{
    internal Environment globals = new();
    public Environment environment;
    private readonly ModuleManager moduleManager = new();
    private Module? currentModule = null;
    
    public Interpreter()
    {
        environment = globals;
        DefineBuiltInFunctions();
    }

    #region Public Interface

    /// <summary>
    /// Sets the current directory for module resolution
    /// </summary>
    public void SetCurrentDirectory(string directory)
    {
        moduleManager.SetCurrentDirectory(directory);
    }

    /// <summary>
    /// Interprets a list of statements
    /// </summary>
    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Runtime error: {ex.Message}");
        }
    }

    /// <summary>
    /// Interprets a single expression
    /// </summary>
    public object? InterpretExpression(Expr expression)
    {
        try
        {
            return Evaluate(expression);
        }
        catch (Exception ex)
        {
            throw new Exception($"Runtime error: {ex.Message}");
        }
    }

    #endregion

    #region Statement Visitors

    public object? VisitExpressionStmt(ExpressionStmt stmt)
    {
        var result = Evaluate(stmt.Expression);
        
        // Don't print "Result:" for void function calls or null results
        if (result != null)
        {
            Console.WriteLine($"Result: {result}");
        }
        return null;
    }

    public object? VisitBlockStmt(BlockStmt stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(environment));
        return null;
    }

    public object? VisitForStmt(ForStmt stmt)
    {
        // Create a new environment for the for loop scope
        Environment previous = environment;
        try
        {
            environment = new Environment(environment);

            // Execute initializer if present
            if (stmt.Initializer != null)
            {
                Execute(stmt.Initializer);
            }

            // Loop while condition is true (or no condition means infinite loop)
            while (stmt.Condition == null || IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);

                // Execute increment if present
                if (stmt.Increment != null)
                {
                    Evaluate(stmt.Increment);
                }
            }
        }
        finally
        {
            environment = previous;
        }

        return null;
    }

    public object? VisitIfStmt(IfStmt stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    public object? VisitVarStmt(VarStmt stmt)
    {
        object? value = stmt.Initializer != null ? Evaluate(stmt.Initializer) : null;

        // Handle type-specific initialization and validation
        if (stmt.ArrayDimensions > 0)
        {
            // Array variable - value should already be an array
            if (value == null)
            {
                throw new Exception("Array variables must be initialized");
            }
        }
        else
        {
            // Regular variable - handle type conversion and defaults
            value = stmt.Type.Type switch
            {
                TokenType.Int => TypeConverter.ConvertToInt(value, 0),
                TokenType.Float => TypeConverter.ConvertToFloat(value, 0.0f),
                TokenType.Char => TypeConverter.ConvertToChar(value, '\0'),
                TokenType.Bool => TypeConverter.ConvertToBool(value, false),
                TokenType.Identifier => value, // Struct type - use the value as-is
                _ => throw new Exception($"Unsupported variable type: {stmt.Type.Type}")
            };
        }

        environment.Define(stmt.Name.Lexeme, value);
        
        // If this is an exported variable and we're in a module, add to exports
        if (stmt.IsExported && currentModule != null)
        {
            currentModule.Exports[stmt.Name.Lexeme] = value;
        }
        
        return null;
    }

    public object? VisitFunctionStmt(FunctionStmt stmt)
    {
        Function function = new(stmt, environment);
        environment.Define(stmt.Name.Lexeme, function);
        
        // If this is an exported function and we're in a module, add to exports
        if (stmt.IsExported && currentModule != null)
        {
            currentModule.Exports[stmt.Name.Lexeme] = function;
        }
        
        return null;
    }

    public object? VisitReturnStmt(ReturnStmt stmt)
    {
        object? value = stmt.Value != null ? Evaluate(stmt.Value) : null;
        throw new ReturnException(value);
    }

    public object? VisitImportStmt(ImportStmt stmt)
    {
        string resolvedPath = moduleManager.ResolvePath(stmt.Path);
        
        if (moduleManager.HasModule(resolvedPath))
        {
            Module existingModule = moduleManager.GetModule(resolvedPath)!;
            if (existingModule.IsLoaded)
            {
                // Module already loaded, import its exports
                ImportModuleExports(existingModule, stmt.ModuleName);
                return null;
            }
        }
        
        // Load and parse the module
        Module module = LoadModule(resolvedPath);
        ImportModuleExports(module, stmt.ModuleName);
        return null;
    }

    public object? VisitStructStmt(StructStmt stmt)
    {
        // Create a struct definition and store it in the environment
        StructDefinition structDef = new(stmt.Name.Lexeme, stmt.Fields, stmt.Methods, stmt.Constructor);
        globals.Define(stmt.Name.Lexeme, structDef);
        
        // If this is exported and we're in a module, add to exports
        if (stmt.IsExported && currentModule != null)
        {
            currentModule.Exports[stmt.Name.Lexeme] = structDef;
        }
        
        return null;
    }

    #endregion

    #region Expression Visitors

    public object? VisitAssignExpr(AssignExpr expr)
    {
        object? value = Evaluate(expr.Value);
        
        // Get the current variable to determine its type
        object? currentValue = environment.Get(expr.Name);
        
        // Convert value to match the variable's type
        value = currentValue switch
        {
            int => TypeConverter.ConvertToInt(value, null),
            float => TypeConverter.ConvertToFloat(value, null),
            char => TypeConverter.ConvertToChar(value, null),
            bool => TypeConverter.ConvertToBool(value, null),
            Array => value, // Arrays are handled separately
            null => value, // Allow assignment to null fields (struct fields, arrays)
            StructInstance => value, // Allow assignment to struct fields
            _ => throw new Exception($"Cannot determine type for assignment to {expr.Name.Lexeme}")
        };

        environment.Assign(expr.Name, value);
        return value;
    }

    public object? VisitBinaryExpr(BinaryExpr expr)
    {
        object? left = Evaluate(expr.Left);
        object? right = Evaluate(expr.Right);

        return expr.Operator.Type switch
        {
            TokenType.OrOr => IsTruthy(left) || IsTruthy(right),
            TokenType.AndAnd => IsTruthy(left) && IsTruthy(right),
            TokenType.EqualEqual => IsEqual(left, right),
            TokenType.BangEqual => !IsEqual(left, right),
            TokenType.Greater => CompareNumbers(left, right, (l, r) => l > r),
            TokenType.GreaterEqual => CompareNumbers(left, right, (l, r) => l >= r),
            TokenType.Less => CompareNumbers(left, right, (l, r) => l < r),
            TokenType.LessEqual => CompareNumbers(left, right, (l, r) => l <= r),
            TokenType.Plus => ArithmeticOperations.Add(left, right),
            TokenType.Minus => ArithmeticOperations.Subtract(left, right),
            TokenType.Multiply => ArithmeticOperations.Multiply(left, right),
            TokenType.Divide => ArithmeticOperations.Divide(left, right),
            TokenType.Modulo => ArithmeticOperations.Modulo(left, right),
            _ => throw new Exception($"Unknown binary operator: {expr.Operator.Lexeme}")
        };
    }

    public object? VisitGroupingExpr(GroupingExpr expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(LiteralExpr expr)
    {
        return expr.Value;
    }

    public object? VisitPostfixExpr(PostfixExpr expr)
    {
        return PostfixOperations.Execute(this, expr);
    }

    public object? VisitUnaryExpr(UnaryExpr expr)
    {
        return UnaryOperations.Execute(this, expr);
    }

    public object? VisitVariableExpr(VariableExpr expr)
    {
        return environment.Get(expr.Name);
    }

    public object? VisitMemberAccessExpr(MemberAccessExpr expr)
    {
        return MemberAccessOperations.Execute(this, expr);
    }

    public object? VisitMemberAssignExpr(MemberAssignExpr expr)
    {
        return MemberAccessOperations.ExecuteAssignment(this, expr);
    }

    public object? VisitStructNewExpr(StructNewExpr expr)
    {
        return StructOperations.CreateInstance(this, expr);
    }

    public object? VisitCallExpr(CallExpr expr)
    {
        return CallOperations.Execute(this, expr);
    }

    public object? VisitArrayAccessExpr(ArrayAccessExpr expr)
    {
        return ArrayOperations.Access(this, expr);
    }

    public object? VisitArrayAssignExpr(ArrayAssignExpr expr)
    {
        return ArrayOperations.Assign(this, expr);
    }

    public object? VisitArrayLiteralExpr(ArrayLiteralExpr expr)
    {
        return ArrayOperations.CreateLiteral(this, expr);
    }

    public object? VisitArrayNewExpr(ArrayNewExpr expr)
    {
        return ArrayOperations.CreateNew(this, expr);
    }

    #endregion

    #region Helper Methods

    public object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    public void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    internal void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        Environment previous = this.environment;
        try
        {
            this.environment = environment;

            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    private bool IsTruthy(object? obj)
    {
        if (obj == null) return false;
        if (obj is bool b) return b;
        return true;
    }

    private bool IsEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;
        return a.Equals(b);
    }

    private object CompareNumbers(object? left, object? right, Func<double, double, bool> compare)
    {
        double leftDouble = TypeConverter.ConvertToDouble(left);
        double rightDouble = TypeConverter.ConvertToDouble(right);
        return compare(leftDouble, rightDouble);
    }

    #endregion

    #region Module System

    private Module LoadModule(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new Exception($"Cannot find module file: {filePath}");
        }
        
        Module module = new(filePath);
        moduleManager.RegisterModule(filePath, module);
        
        // Read and parse the file
        string source = File.ReadAllText(filePath);
        Lexer lexer = new(source);
        List<Token> tokens = lexer.ScanTokens();
        Parser parser = new(tokens);
        List<Stmt> statements = parser.Parse();
        
        // Set current directory for nested imports
        string previousDirectory = moduleManager.GetCurrentDirectory();
        moduleManager.SetCurrentDirectory(Path.GetDirectoryName(filePath) ?? "");
        
        // Create a new environment for the module
        Environment previousEnv = environment;
        Module? previousModule = currentModule;
        environment = new Environment(globals);
        currentModule = module;
        
        try
        {
            // Execute the module statements
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
            
            module.IsLoaded = true;
        }
        finally
        {
            // Restore previous environment and directory
            environment = previousEnv;
            moduleManager.SetCurrentDirectory(previousDirectory);
            currentModule = previousModule;
        }
        
        return module;
    }

    private void ImportModuleExports(Module module, string? moduleName)
    {
        if (moduleName != null)
        {
            // Named import: create a module object with all exports
            Dictionary<string, object?> moduleObject = new();
            foreach (var export in module.Exports)
            {
                moduleObject[export.Key] = export.Value;
            }
            environment.Define(moduleName, moduleObject);
        }
        else
        {
            // Direct import: add all exports to current scope
            foreach (var export in module.Exports)
            {
                environment.Define(export.Key, export.Value);
            }
        }
    }

    #endregion

    #region Built-in Functions

    private void DefineBuiltInFunctions()
    {
        // __put(char c) - outputs a single character to stdout
        globals.Define("__put", new BuiltInFunction("__put", 1, (interpreter, arguments) =>
        {
            object? arg = arguments[0];
            
            // Convert to char if necessary
            char c = arg switch
            {
                char ch => ch,
                int i when i >= 0 && i <= 65535 => (char)i,
                _ => throw new Exception($"__put expects a char argument, got {arg?.GetType().Name}")
            };
            
            Console.Write(c);
            return null; // void return type
        }));
    }

    #endregion

    #region Type Conversion Methods (for compatibility)

    public object ConvertToInt(object? value, int? defaultValue)
    {
        return TypeConverter.ConvertToInt(value, defaultValue);
    }

    public object ConvertToFloat(object? value, float? defaultValue)
    {
        return TypeConverter.ConvertToFloat(value, defaultValue);
    }

    public object ConvertToChar(object? value, char? defaultValue)
    {
        return TypeConverter.ConvertToChar(value, defaultValue);
    }

    public object ConvertToBool(object? value, bool? defaultValue)
    {
        return TypeConverter.ConvertToBool(value, defaultValue);
    }

    #endregion
}