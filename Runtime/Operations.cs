namespace Calculator.Runtime;

/// <summary>
/// Handles postfix operations (++ and --)
/// </summary>
public static class PostfixOperations
{
    public static object? Execute(Interpreter interpreter, PostfixExpr expr)
    {
        if (expr.Expression is VariableExpr variable)
        {
            object? currentValue = interpreter.environment.Get(variable.Name);
            
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

            interpreter.environment.Assign(variable.Name, newValue);
            return currentValue; // Return the original value (postfix semantics)
        }
        else if (expr.Expression is ArrayAccessExpr arrayAccess)
        {
            object? currentValue = interpreter.Evaluate(arrayAccess);
            
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
            interpreter.Evaluate(assignment);
            
            return currentValue; // Return the original value (postfix semantics)
        }
        else
        {
            throw new Exception("Invalid target for postfix operator");
        }
    }
}

/// <summary>
/// Handles unary operations
/// </summary>
public static class UnaryOperations
{
    public static object? Execute(Interpreter interpreter, UnaryExpr expr)
    {
        // Handle prefix increment/decrement operators
        if (expr.Operator.Type == TokenType.PlusPlus || expr.Operator.Type == TokenType.MinusMinus)
        {
            if (expr.Right is VariableExpr variable)
            {
                object? currentValue = interpreter.environment.Get(variable.Name);
                
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

                interpreter.environment.Assign(variable.Name, newValue);
                return newValue; // Return the new value (prefix semantics)
            }
            else if (expr.Right is ArrayAccessExpr arrayAccess)
            {
                object? currentValue = interpreter.Evaluate(arrayAccess);
                
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
                interpreter.Evaluate(assignment);
                
                return newValue; // Return the new value (prefix semantics)
            }
            else
            {
                throw new Exception("Invalid target for prefix operator");
            }
        }

        // Handle regular unary operators (+ and -)
        object? right = interpreter.Evaluate(expr.Right);

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
}

/// <summary>
/// Handles member access operations
/// </summary>
public static class MemberAccessOperations
{
    public static object? Execute(Interpreter interpreter, MemberAccessExpr expr)
    {
        object? target;
        string targetName;
        
        if (expr.ModuleName != null)
        {
            // Traditional module.member access
            target = interpreter.environment.Get(expr.ModuleName);
            targetName = expr.ModuleName.Lexeme;
        }
        else if (expr.Target != null)
        {
            // Expression.member access (e.g., array[0].member)
            target = interpreter.Evaluate(expr.Target);
            targetName = "expression";
        }
        else
        {
            throw new Exception($"Invalid member access expression at position {expr.MemberName.Position}");
        }
        
        if (target is Dictionary<string, object?> module)
        {
            // Module access
            if (!module.TryGetValue(expr.MemberName.Lexeme, out object? member))
            {
                throw new Exception($"Module '{targetName}' does not export '{expr.MemberName.Lexeme}' at position {expr.MemberName.Position}");
            }
            return member;
        }
        else if (target is StructInstance structInstance)
        {
            // Struct field access
            if (structInstance.Fields.TryGetValue(expr.MemberName.Lexeme, out object? fieldValue))
            {
                return fieldValue;
            }
            
            // Struct method access
            foreach (var method in structInstance.Definition.Methods)
            {
                if (method.Name.Lexeme == expr.MemberName.Lexeme)
                {
                    // Return a bound method (method with 'this' context)
                    return new BoundMethod(structInstance, method);
                }
            }
            
            throw new Exception($"Struct '{structInstance.Definition.Name}' does not have field or method '{expr.MemberName.Lexeme}' at position {expr.MemberName.Position}");
        }
        else
        {
            throw new Exception($"'{targetName}' is not a module or struct instance at position {expr.MemberName.Position}");
        }
    }

    public static object? ExecuteAssignment(Interpreter interpreter, MemberAssignExpr expr)
    {
        object? value = interpreter.Evaluate(expr.Value);
        
        // Get the target object (either from module name or expression)
        object? target;
        if (expr.Target.ModuleName != null)
        {
            target = interpreter.environment.Get(expr.Target.ModuleName);
        }
        else if (expr.Target.Target != null)
        {
            target = interpreter.Evaluate(expr.Target.Target);
        }
        else
        {
            throw new Exception($"Invalid member assignment target at position {expr.Target.MemberName.Position}");
        }
        
        if (target is StructInstance structInstance)
        {
            // Assign to struct field
            if (structInstance.Fields.ContainsKey(expr.Target.MemberName.Lexeme))
            {
                structInstance.Fields[expr.Target.MemberName.Lexeme] = value;
                return value;
            }
            else
            {
                throw new Exception($"Struct '{structInstance.Definition.Name}' does not have field '{expr.Target.MemberName.Lexeme}' at position {expr.Target.MemberName.Position}");
            }
        }
        else
        {
            throw new Exception($"Cannot assign to member of non-struct type at position {expr.Target.MemberName.Position}");
        }
    }
}

/// <summary>
/// Handles struct operations
/// </summary>
public static class StructOperations
{
    public static object? CreateInstance(Interpreter interpreter, StructNewExpr expr)
    {
        object? structDefObj = interpreter.globals.Get(new Token(TokenType.Identifier, expr.StructType.Lexeme, null, expr.StructType.Position));
        
        if (structDefObj is not StructDefinition structDef)
        {
            throw new Exception($"'{expr.StructType.Lexeme}' is not a struct type at position {expr.StructType.Position}");
        }
        
        // Create new struct instance
        StructInstance instance = new StructInstance(structDef);
        
        // Call constructor if it exists
        if (structDef.Constructor != null)
        {
            // Create a new environment for the constructor
            Environment previous = interpreter.environment;
            interpreter.environment = new Environment(interpreter.globals);
            
            // Define 'this' to refer to the struct instance
            interpreter.environment.Define("this", instance);
            
            // Make struct fields accessible directly in constructor
            foreach (var field in instance.Fields)
            {
                interpreter.environment.Define(field.Key, field.Value);
            }
            
            try
            {
                // Execute constructor body
                interpreter.ExecuteBlock(structDef.Constructor.Body, interpreter.environment);
                
                // Copy modified field values back to the instance
                foreach (var field in structDef.Fields)
                {
                    try
                    {
                        object? newValue = interpreter.environment.GetByName(field.Name.Lexeme);
                        instance.Fields[field.Name.Lexeme] = newValue;
                    }
                    catch
                    {
                        // Field wasn't modified in constructor, keep original value
                    }
                }
            }
            catch (ReturnException)
            {
                // Constructors don't return values, so ignore return exceptions
            }
            finally
            {
                interpreter.environment = previous;
            }
        }
        
        return instance;
    }
}

/// <summary>
/// Handles function call operations
/// </summary>
public static class CallOperations
{
    public static object? Execute(Interpreter interpreter, CallExpr expr)
    {
        object? callee = interpreter.Evaluate(expr.Callee);
        
        if (callee is not ICallable callable)
        {
            string calleeDescription = expr.Callee switch
            {
                VariableExpr varExpr => varExpr.Name.Lexeme,
                MemberAccessExpr memberExpr => $"{memberExpr.ModuleName?.Lexeme}.{memberExpr.MemberName.Lexeme}",
                _ => "expression"
            };
            throw new Exception($"'{calleeDescription}' is not a function");
        }
        
        List<object?> arguments = new();
        foreach (Expr argument in expr.Arguments)
        {
            arguments.Add(interpreter.Evaluate(argument));
        }
        
        if (arguments.Count != callable.Arity)
        {
            throw new Exception($"Expected {callable.Arity} arguments but got {arguments.Count}");
        }
        
        return callable.Call(interpreter, arguments);
    }
}