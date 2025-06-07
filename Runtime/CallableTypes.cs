namespace Calculator.Runtime;

/// <summary>
/// Interface for callable objects (functions, methods, built-ins)
/// </summary>
public interface ICallable
{
    int Arity { get; }
    object? Call(Interpreter interpreter, List<object?> arguments);
}

/// <summary>
/// User-defined function implementation
/// </summary>
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
                        TokenType.String => interpreter.ConvertToString(arg), // String reference type
                        TokenType.Identifier => CreateParameterValue(arg), // Handle value vs reference semantics
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
                
                // Special validation for void functions
                if (Declaration.ReturnType.Type == TokenType.Void)
                {
                    if (result != null)
                    {
                        throw new Exception($"Void function '{Declaration.Name.Lexeme}' cannot return a value");
                    }
                    return VoidResult.Instance;
                }
                
                if (Declaration.ReturnArrayDimensions == 0 && result != null)
                {
                    result = Declaration.ReturnType.Type switch
                    {
                        TokenType.Int => interpreter.ConvertToInt(result, null),
                        TokenType.Float => interpreter.ConvertToFloat(result, null),
                        TokenType.Char => interpreter.ConvertToChar(result, null),
                        TokenType.Bool => interpreter.ConvertToBool(result, null),
                        TokenType.String => interpreter.ConvertToString(result),
                        TokenType.Identifier => result, // Struct types - return as-is
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
                    TokenType.String => StringInstance.Create("", interpreter.HeapManager),
                    TokenType.Void => VoidResult.Instance,
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

    /// <summary>
    /// Creates the appropriate parameter value based on type semantics
    /// Structs: value semantics (copy), Classes: reference semantics (same object)
    /// </summary>
    private static object? CreateParameterValue(object? arg)
    {
        return arg switch
        {
            StructInstance structInstance => CreateStructCopy(structInstance),
            ClassInstance classInstance => classInstance, // Reference semantics - pass same object
            _ => arg // Other types (null, primitives, etc.) - pass as-is
        };
    }

    /// <summary>
    /// Creates a shallow copy of a struct instance for value semantics
    /// </summary>
    private static StructInstance CreateStructCopy(StructInstance original)
    {
        // Create a new struct instance with the same definition
        StructInstance copy = new StructInstance(original.Definition);
        
        // Copy all field values
        foreach (var field in original.Fields)
        {
            copy.Fields[field.Key] = field.Value;
        }
        
        return copy;
    }
}

/// <summary>
/// Built-in function implementation
/// </summary>
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

/// <summary>
/// Exception thrown by return statements
/// </summary>
public class ReturnException : Exception
{
    public object? Value { get; }

    public ReturnException(object? value) : base()
    {
        Value = value;
    }
}