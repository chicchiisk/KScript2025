namespace Calculator;

public interface ICallable
{
    int Arity { get; }
    object? Call(Interpreter interpreter, List<object?> arguments);
}

public class Function : ICallable
{
    public FunctionStmt Declaration { get; }
    public Environment Closure { get; }

    public Function(FunctionStmt declaration, Environment closure)
    {
        Declaration = declaration;
        Closure = closure;
    }

    public int Arity => Declaration.Parameters.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        Environment previous = interpreter.environment;
        
        try
        {
            // Create new environment for function scope
            interpreter.environment = new Environment(Closure);
            
            // Bind parameters
            for (int i = 0; i < Declaration.Parameters.Count; i++)
            {
                Parameter param = Declaration.Parameters[i];
                object? arg = arguments[i];
                
                // Convert argument to parameter type
                if (param.ArrayDimensions == 0)
                {
                    arg = param.Type.Type switch
                    {
                        TokenType.Int => interpreter.ConvertToInt(arg, null),
                        TokenType.Float => interpreter.ConvertToFloat(arg, null),
                        TokenType.Char => interpreter.ConvertToChar(arg, null),
                        TokenType.Bool => interpreter.ConvertToBool(arg, null),
                        _ => throw new Exception($"Unsupported parameter type: {param.Type.Type}")
                    };
                }
                
                interpreter.environment.Define(param.Name.Lexeme, arg);
            }
            
            // Execute function body
            try
            {
                foreach (Stmt stmt in Declaration.Body)
                {
                    interpreter.Execute(stmt);
                }
            }
            catch (ReturnException returnValue)
            {
                // Convert return value to function return type
                object? result = returnValue.Value;
                if (Declaration.ReturnArrayDimensions == 0 && result != null)
                {
                    result = Declaration.ReturnType.Type switch
                    {
                        TokenType.Int => interpreter.ConvertToInt(result, null),
                        TokenType.Float => interpreter.ConvertToFloat(result, null),
                        TokenType.Char => interpreter.ConvertToChar(result, null),
                        TokenType.Bool => interpreter.ConvertToBool(result, null),
                        TokenType.Void => null,
                        _ => throw new Exception($"Unsupported return type: {Declaration.ReturnType.Type}")
                    };
                }
                return result;
            }
            
            // If no return statement, return default value for type
            if (Declaration.ReturnArrayDimensions == 0)
            {
                return Declaration.ReturnType.Type switch
                {
                    TokenType.Int => 0,
                    TokenType.Float => 0.0f,
                    TokenType.Char => '\0',
                    TokenType.Bool => false,
                    TokenType.Void => null,
                    _ => null
                };
            }
            
            return null;
        }
        finally
        {
            interpreter.environment = previous;
        }
    }
}

public class BuiltInFunction : ICallable
{
    public string Name { get; }
    public int Arity { get; }
    private readonly Func<Interpreter, List<object?>, object?> implementation;

    public BuiltInFunction(string name, int arity, Func<Interpreter, List<object?>, object?> implementation)
    {
        Name = name;
        Arity = arity;
        this.implementation = implementation;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        return implementation(interpreter, arguments);
    }
}

public class ReturnException : Exception
{
    public object? Value { get; }

    public ReturnException(object? value) : base()
    {
        Value = value;
    }
}

public class Interpreter : IExprVisitor<object?>, IStmtVisitor<object?>
{
    private Environment globals = new();
    public Environment environment;
    
    public Interpreter()
    {
        environment = globals;
        DefineBuiltInFunctions();
    }

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
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

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
                TokenType.Int => ConvertToInt(value, 0),
                TokenType.Float => ConvertToFloat(value, 0.0f),
                TokenType.Char => ConvertToChar(value, '\0'),
                TokenType.Bool => ConvertToBool(value, false),
                _ => throw new Exception($"Unsupported variable type: {stmt.Type.Type}")
            };
        }

        environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitFunctionStmt(FunctionStmt stmt)
    {
        Function function = new(stmt, environment);
        environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitReturnStmt(ReturnStmt stmt)
    {
        object? value = null;
        if (stmt.Value != null)
        {
            value = Evaluate(stmt.Value);
        }
        
        throw new ReturnException(value);
    }

    public object? VisitAssignExpr(AssignExpr expr)
    {
        object? value = Evaluate(expr.Value);
        
        // Get the current variable to determine its type
        object? currentValue = environment.Get(expr.Name);
        
        // Convert value to match the variable's type
        value = currentValue switch
        {
            int => ConvertToInt(value, null),
            float => ConvertToFloat(value, null),
            char => ConvertToChar(value, null),
            bool => ConvertToBool(value, null),
            Array => value, // Arrays are handled separately
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
            TokenType.Plus => AddNumbers(left, right),
            TokenType.Minus => SubtractNumbers(left, right),
            TokenType.Multiply => MultiplyNumbers(left, right),
            TokenType.Divide => DivideNumbers(left, right),
            TokenType.Modulo => ModuloNumbers(left, right),
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
        if (expr.Expression is VariableExpr variable)
        {
            object? currentValue = environment.Get(variable.Name);
            
            object? newValue = (currentValue, expr.Operator.Type) switch
            {
                (int intValue, TokenType.PlusPlus) => intValue + 1,
                (int intValue, TokenType.MinusMinus) => intValue - 1,
                (float floatValue, TokenType.PlusPlus) => floatValue + 1.0f,
                (float floatValue, TokenType.MinusMinus) => floatValue - 1.0f,
                (char charValue, TokenType.PlusPlus) => (char)(charValue + 1),
                (char charValue, TokenType.MinusMinus) => (char)(charValue - 1),
                _ => throw new Exception($"Postfix operators can only be applied to numeric or char variables, got {currentValue?.GetType().Name}")
            };

            environment.Assign(variable.Name, newValue);
            
            // Return the original value (postfix semantics)
            return currentValue;
        }
        else if (expr.Expression is ArrayAccessExpr arrayAccess)
        {
            object? currentValue = Evaluate(arrayAccess);
            
            object? newValue = (currentValue, expr.Operator.Type) switch
            {
                (int intValue, TokenType.PlusPlus) => intValue + 1,
                (int intValue, TokenType.MinusMinus) => intValue - 1,
                (float floatValue, TokenType.PlusPlus) => floatValue + 1.0f,
                (float floatValue, TokenType.MinusMinus) => floatValue - 1.0f,
                (char charValue, TokenType.PlusPlus) => (char)(charValue + 1),
                (char charValue, TokenType.MinusMinus) => (char)(charValue - 1),
                _ => throw new Exception($"Postfix operators can only be applied to numeric or char values, got {currentValue?.GetType().Name}")
            };

            // Update the array element
            var assignment = new ArrayAssignExpr(arrayAccess, new LiteralExpr(newValue));
            Evaluate(assignment);
            
            // Return the original value (postfix semantics)
            return currentValue;
        }
        else
        {
            throw new Exception("Invalid target for postfix operator");
        }
    }

    public object? VisitUnaryExpr(UnaryExpr expr)
    {
        // Handle prefix increment/decrement operators
        if (expr.Operator.Type == TokenType.PlusPlus || expr.Operator.Type == TokenType.MinusMinus)
        {
            if (expr.Right is VariableExpr variable)
            {
                object? currentValue = environment.Get(variable.Name);
                
                object? newValue = (currentValue, expr.Operator.Type) switch
                {
                    (int intValue, TokenType.PlusPlus) => intValue + 1,
                    (int intValue, TokenType.MinusMinus) => intValue - 1,
                    (float floatValue, TokenType.PlusPlus) => floatValue + 1.0f,
                    (float floatValue, TokenType.MinusMinus) => floatValue - 1.0f,
                    (char charValue, TokenType.PlusPlus) => (char)(charValue + 1),
                    (char charValue, TokenType.MinusMinus) => (char)(charValue - 1),
                    _ => throw new Exception($"Prefix operators can only be applied to numeric or char variables, got {currentValue?.GetType().Name}")
                };

                environment.Assign(variable.Name, newValue);
                
                // Return the new value (prefix semantics)
                return newValue;
            }
            else if (expr.Right is ArrayAccessExpr arrayAccess)
            {
                object? currentValue = Evaluate(arrayAccess);
                
                object? newValue = (currentValue, expr.Operator.Type) switch
                {
                    (int intValue, TokenType.PlusPlus) => intValue + 1,
                    (int intValue, TokenType.MinusMinus) => intValue - 1,
                    (float floatValue, TokenType.PlusPlus) => floatValue + 1.0f,
                    (float floatValue, TokenType.MinusMinus) => floatValue - 1.0f,
                    (char charValue, TokenType.PlusPlus) => (char)(charValue + 1),
                    (char charValue, TokenType.MinusMinus) => (char)(charValue - 1),
                    _ => throw new Exception($"Prefix operators can only be applied to numeric or char values, got {currentValue?.GetType().Name}")
                };

                // Update the array element
                var assignment = new ArrayAssignExpr(arrayAccess, new LiteralExpr(newValue));
                Evaluate(assignment);
                
                // Return the new value (prefix semantics)
                return newValue;
            }
            else
            {
                throw new Exception("Invalid target for prefix operator");
            }
        }

        // Handle regular unary operators (+ and -)
        object? right = Evaluate(expr.Right);

        return right switch
        {
            int rightInt => expr.Operator.Type switch
            {
                TokenType.Minus => -rightInt,
                TokenType.Plus => rightInt,
                _ => throw new Exception($"Unknown unary operator: {expr.Operator.Lexeme}")
            },
            float rightFloat => expr.Operator.Type switch
            {
                TokenType.Minus => -rightFloat,
                TokenType.Plus => rightFloat,
                _ => throw new Exception($"Unknown unary operator: {expr.Operator.Lexeme}")
            },
            double rightDouble => expr.Operator.Type switch
            {
                TokenType.Minus => -rightDouble,
                TokenType.Plus => rightDouble,
                _ => throw new Exception($"Unknown unary operator: {expr.Operator.Lexeme}")
            },
            _ => throw new Exception($"Incompatible type for unary operation: {right?.GetType()}")
        };
    }

    public object? VisitVariableExpr(VariableExpr expr)
    {
        return environment.Get(expr.Name);
    }

    public object? VisitCallExpr(CallExpr expr)
    {
        object? callee = environment.Get(expr.Name);
        
        if (callee is not ICallable callable)
        {
            throw new Exception($"'{expr.Name.Lexeme}' is not a function");
        }
        
        List<object?> arguments = new();
        foreach (Expr argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }
        
        if (arguments.Count != callable.Arity)
        {
            throw new Exception($"Expected {callable.Arity} arguments but got {arguments.Count}");
        }
        
        return callable.Call(this, arguments);
    }


    public object? VisitArrayAccessExpr(ArrayAccessExpr expr)
    {
        object? arrayObj = Evaluate(expr.Array);
        
        if (arrayObj is not Array array)
        {
            throw new Exception("Cannot index non-array value");
        }

        List<int> indices = new();
        foreach (Expr indexExpr in expr.Indices)
        {
            object? indexObj = Evaluate(indexExpr);
            if (indexObj is not int index)
            {
                throw new Exception("Array index must be an integer");
            }
            indices.Add(index);
        }

        try
        {
            return indices.Count switch
            {
                1 => array.GetValue(indices[0]),
                2 => array.GetValue(indices[0], indices[1]),
                3 => array.GetValue(indices[0], indices[1], indices[2]),
                _ => throw new Exception($"Arrays with {indices.Count} dimensions not supported")
            };
        }
        catch (IndexOutOfRangeException)
        {
            throw new Exception("Array index out of bounds");
        }
    }

    public object? VisitArrayAssignExpr(ArrayAssignExpr expr)
    {
        object? value = Evaluate(expr.Value);
        object? arrayObj = Evaluate(expr.Target.Array);
        
        if (arrayObj is not Array array)
        {
            throw new Exception("Cannot index non-array value");
        }

        List<int> indices = new();
        foreach (Expr indexExpr in expr.Target.Indices)
        {
            object? indexObj = Evaluate(indexExpr);
            if (indexObj is not int index)
            {
                throw new Exception("Array index must be an integer");
            }
            indices.Add(index);
        }

        // Convert value to match array element type
        Type arrayElementType = array.GetType().GetElementType()!;
        value = arrayElementType.Name switch
        {
            "Int32" => ConvertToInt(value, null),
            "Single" => ConvertToFloat(value, null),
            "Char" => ConvertToChar(value, null),
            "Boolean" => ConvertToBool(value, null),
            _ => throw new Exception($"Unsupported array element type: {arrayElementType.Name}")
        };

        try
        {
            switch (indices.Count)
            {
                case 1:
                    array.SetValue(value, indices[0]);
                    break;
                case 2:
                    array.SetValue(value, indices[0], indices[1]);
                    break;
                case 3:
                    array.SetValue(value, indices[0], indices[1], indices[2]);
                    break;
                default:
                    throw new Exception($"Arrays with {indices.Count} dimensions not supported");
            }
        }
        catch (IndexOutOfRangeException)
        {
            throw new Exception("Array index out of bounds");
        }

        return value;
    }

    public object? VisitArrayLiteralExpr(ArrayLiteralExpr expr)
    {
        if (expr.Elements.Count == 0)
        {
            // Empty array - default to int[]
            return new int[0];
        }

        List<object?> values = new();
        foreach (Expr element in expr.Elements)
        {
            object? value = Evaluate(element);
            values.Add(value);
        }

        // Check if this is a multi-dimensional array (contains nested arrays)
        if (values.Count > 0 && values[0] is Array)
        {
            // Multi-dimensional array - determine element type from first element of first row
            Array firstRow = (Array)values[0]!;
            int rows = values.Count;
            int cols = firstRow.Length;
            
            // Validate all rows have same length
            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] is not Array row || row.Length != cols)
                {
                    throw new Exception("All rows in multi-dimensional array must have same length");
                }
            }
            
            // Determine type from first element
            if (cols > 0)
            {
                object? firstElement = firstRow.GetValue(0);
                return firstElement switch
                {
                    int => Create2DIntArray(values, rows, cols),
                    float => Create2DFloatArray(values, rows, cols),
                    char => Create2DCharArray(values, rows, cols),
                    bool => Create2DBoolArray(values, rows, cols),
                    _ => throw new Exception($"Unsupported array element type: {firstElement?.GetType().Name}")
                };
            }
            
            // Empty rows, default to int
            return new int[rows, cols];
        }
        else
        {
            // Single-dimensional array - determine type from first element
            object? firstElement = values[0];
            return firstElement switch
            {
                int => Create1DIntArray(values),
                float => Create1DFloatArray(values),
                char => Create1DCharArray(values),
                bool => Create1DBoolArray(values),
                _ => throw new Exception($"Unsupported array element type: {firstElement?.GetType().Name}")
            };
        }
    }

    public object? VisitArrayNewExpr(ArrayNewExpr expr)
    {
        List<int> dimensions = new();
        foreach (Expr dimExpr in expr.Dimensions)
        {
            object? dimObj = Evaluate(dimExpr);
            if (dimObj is not int dim)
            {
                throw new Exception("Array dimension must be an integer");
            }
            if (dim < 0)
            {
                throw new Exception("Array dimension cannot be negative");
            }
            dimensions.Add(dim);
        }

        return (expr.Type.Type, dimensions.Count) switch
        {
            (TokenType.Int, 1) => new int[dimensions[0]],
            (TokenType.Int, 2) => new int[dimensions[0], dimensions[1]],
            (TokenType.Int, 3) => new int[dimensions[0], dimensions[1], dimensions[2]],
            (TokenType.Float, 1) => new float[dimensions[0]],
            (TokenType.Float, 2) => new float[dimensions[0], dimensions[1]],
            (TokenType.Float, 3) => new float[dimensions[0], dimensions[1], dimensions[2]],
            (TokenType.Char, 1) => new char[dimensions[0]],
            (TokenType.Char, 2) => new char[dimensions[0], dimensions[1]],
            (TokenType.Char, 3) => new char[dimensions[0], dimensions[1], dimensions[2]],
            (TokenType.Bool, 1) => new bool[dimensions[0]],
            (TokenType.Bool, 2) => new bool[dimensions[0], dimensions[1]],
            (TokenType.Bool, 3) => new bool[dimensions[0], dimensions[1], dimensions[2]],
            _ => throw new Exception($"Arrays with {dimensions.Count} dimensions not supported for type {expr.Type.Type}")
        };
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    public void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void ExecuteBlock(List<Stmt> statements, Environment environment)
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
        double leftDouble = ConvertToDouble(left);
        double rightDouble = ConvertToDouble(right);
        return compare(leftDouble, rightDouble);
    }

    private object AddNumbers(object? left, object? right)
    {
        // If both are int, return int
        if (left is int leftInt && right is int rightInt)
            return leftInt + rightInt;
        
        // If either operand is char, handle char arithmetic
        if (left is char leftChar && right is int rightIntForChar)
            return (char)(leftChar + rightIntForChar);
        if (left is int leftIntForChar && right is char rightChar)
            return (char)(leftIntForChar + rightChar);
        if (left is char leftChar2 && right is char rightChar2)
            return (char)(leftChar2 + rightChar2);
        
        // If either is float, return float
        if ((left is float || right is float) && 
            (left is int || left is float) && (right is int || right is float))
        {
            float leftFloat = left is float lf ? lf : (int)left!;
            float rightFloat = right is float rf ? rf : (int)right!;
            return leftFloat + rightFloat;
        }

        // Otherwise use double for compatibility
        double leftDouble = ConvertToDouble(left);
        double rightDouble = ConvertToDouble(right);
        return leftDouble + rightDouble;
    }

    private object SubtractNumbers(object? left, object? right)
    {
        if (left is int leftInt && right is int rightInt)
            return leftInt - rightInt;
        
        if ((left is float || right is float) && 
            (left is int || left is float) && (right is int || right is float))
        {
            float leftFloat = left is float lf ? lf : (int)left!;
            float rightFloat = right is float rf ? rf : (int)right!;
            return leftFloat - rightFloat;
        }

        double leftDouble = ConvertToDouble(left);
        double rightDouble = ConvertToDouble(right);
        return leftDouble - rightDouble;
    }

    private object MultiplyNumbers(object? left, object? right)
    {
        if (left is int leftInt && right is int rightInt)
            return leftInt * rightInt;
        
        if ((left is float || right is float) && 
            (left is int || left is float) && (right is int || right is float))
        {
            float leftFloat = left is float lf ? lf : (int)left!;
            float rightFloat = right is float rf ? rf : (int)right!;
            return leftFloat * rightFloat;
        }

        double leftDouble = ConvertToDouble(left);
        double rightDouble = ConvertToDouble(right);
        return leftDouble * rightDouble;
    }

    private object DivideNumbers(object? left, object? right)
    {
        if (left is int leftInt && right is int rightInt)
            return rightInt != 0 ? leftInt / rightInt : throw new Exception("Division by zero");

        if ((left is float || right is float) && 
            (left is int || left is float) && (right is int || right is float))
        {
            float leftFloat = left is float lf ? lf : (int)left!;
            float rightFloat = right is float rf ? rf : (int)right!;
            return rightFloat != 0 ? leftFloat / rightFloat : throw new Exception("Division by zero");
        }

        double leftDouble = ConvertToDouble(left);
        double rightDouble = ConvertToDouble(right);
        return rightDouble != 0 ? leftDouble / rightDouble : throw new Exception("Division by zero");
    }

    private object ModuloNumbers(object? left, object? right)
    {
        if (left is int leftInt && right is int rightInt)
            return rightInt != 0 ? leftInt % rightInt : throw new Exception("Modulo by zero");

        if ((left is float || right is float) && 
            (left is int || left is float) && (right is int || right is float))
        {
            float leftFloat = left is float lf ? lf : (int)left!;
            float rightFloat = right is float rf ? rf : (int)right!;
            return rightFloat != 0 ? leftFloat % rightFloat : throw new Exception("Modulo by zero");
        }

        double leftDouble = ConvertToDouble(left);
        double rightDouble = ConvertToDouble(right);
        return rightDouble != 0 ? leftDouble % rightDouble : throw new Exception("Modulo by zero");
    }

    private double ConvertToDouble(object? value)
    {
        return value switch
        {
            int i => (double)i,
            double d => d,
            float f => (double)f,
            char c => (double)c,
            _ => throw new Exception($"Invalid numeric type: {value?.GetType()}")
        };
    }

    public object ConvertToInt(object? value, int? defaultValue)
    {
        if (value == null)
        {
            if (defaultValue.HasValue)
                return defaultValue.Value;
            throw new Exception("Cannot assign null to int variable");
        }

        return value switch
        {
            int i => i,
            float f => (int)f,
            double d => (int)d,
            char c => (int)c,
            bool b => b ? 1 : 0,
            _ => throw new Exception($"Cannot convert {value.GetType().Name} to int")
        };
    }

    public object ConvertToFloat(object? value, float? defaultValue)
    {
        if (value == null)
        {
            if (defaultValue.HasValue)
                return defaultValue.Value;
            throw new Exception("Cannot assign null to float variable");
        }

        return value switch
        {
            float f => f,
            int i => (float)i,
            double d => (float)d,
            char c => (float)c,
            _ => throw new Exception($"Cannot convert {value.GetType().Name} to float")
        };
    }

    public object ConvertToChar(object? value, char? defaultValue)
    {
        if (value == null)
        {
            if (defaultValue.HasValue)
                return defaultValue.Value;
            throw new Exception("Cannot assign null to char variable");
        }

        return value switch
        {
            char c => c,
            int i when i >= 0 && i <= 65535 => (char)i,
            _ => throw new Exception($"Cannot convert {value.GetType().Name} to char")
        };
    }

    public object ConvertToBool(object? value, bool? defaultValue)
    {
        if (value == null)
        {
            if (defaultValue.HasValue)
                return defaultValue.Value;
            throw new Exception("Cannot assign null to bool variable");
        }

        return value switch
        {
            bool b => b,
            int i => i != 0,
            float f => f != 0.0f,
            double d => d != 0.0,
            _ => throw new Exception($"Cannot convert {value.GetType().Name} to bool")
        };
    }

    private int[] Create1DIntArray(List<object?> values)
    {
        int[] result = new int[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (int)ConvertToInt(values[i], null);
        }
        return result;
    }

    private float[] Create1DFloatArray(List<object?> values)
    {
        float[] result = new float[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (float)ConvertToFloat(values[i], null);
        }
        return result;
    }

    private char[] Create1DCharArray(List<object?> values)
    {
        char[] result = new char[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (char)ConvertToChar(values[i], null);
        }
        return result;
    }

    private bool[] Create1DBoolArray(List<object?> values)
    {
        bool[] result = new bool[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (bool)ConvertToBool(values[i], null);
        }
        return result;
    }

    private int[,] Create2DIntArray(List<object?> values, int rows, int cols)
    {
        int[,] result = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (int)ConvertToInt(row.GetValue(j), null);
            }
        }
        return result;
    }

    private float[,] Create2DFloatArray(List<object?> values, int rows, int cols)
    {
        float[,] result = new float[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (float)ConvertToFloat(row.GetValue(j), null);
            }
        }
        return result;
    }

    private char[,] Create2DCharArray(List<object?> values, int rows, int cols)
    {
        char[,] result = new char[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (char)ConvertToChar(row.GetValue(j), null);
            }
        }
        return result;
    }

    private bool[,] Create2DBoolArray(List<object?> values, int rows, int cols)
    {
        bool[,] result = new bool[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (bool)ConvertToBool(row.GetValue(j), null);
            }
        }
        return result;
    }
}