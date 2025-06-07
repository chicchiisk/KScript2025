namespace Calculator.Runtime;

/// <summary>
/// Represents a struct definition (template)
/// </summary>
public class StructDefinition
{
    public string Name { get; }
    public List<VarStmt> Fields { get; }
    public List<FunctionStmt> Methods { get; }
    public FunctionStmt? Constructor { get; }

    public StructDefinition(string name, List<VarStmt> fields, List<FunctionStmt> methods, FunctionStmt? constructor)
    {
        Name = name;
        Fields = fields;
        Methods = methods;
        Constructor = constructor;
    }
}

/// <summary>
/// Represents an instance of a struct
/// </summary>
public class StructInstance
{
    public StructDefinition Definition { get; }
    public Dictionary<string, object?> Fields { get; }

    public StructInstance(StructDefinition definition)
    {
        Definition = definition;
        Fields = new Dictionary<string, object?>();
        
        // Initialize fields with default values
        foreach (var field in definition.Fields)
        {
            object? defaultValue;
            if (field.ArrayDimensions > 0)
            {
                // Array field - initialize to null (must be explicitly assigned)
                defaultValue = null;
            }
            else
            {
                // Regular field - use type defaults
                defaultValue = field.Type.Type switch
                {
                    TokenType.Int => 0,
                    TokenType.Float => 0.0f,
                    TokenType.Char => '\0',
                    TokenType.Bool => false,
                    TokenType.Identifier => null, // Struct type
                    _ => null
                };
            }
            Fields[field.Name.Lexeme] = defaultValue;
        }
    }
}

/// <summary>
/// Represents a method bound to a struct instance
/// </summary>
public class BoundMethod : ICallable
{
    public StructInstance Instance { get; }
    public FunctionStmt Method { get; }

    public BoundMethod(StructInstance instance, FunctionStmt method)
    {
        Instance = instance;
        Method = method;
    }

    public int Arity => Method.Parameters.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        // Create a new environment for the method
        Environment previous = interpreter.environment;
        interpreter.environment = new Environment(interpreter.globals);
        
        // Define 'this' to refer to the struct instance
        interpreter.environment.Define("this", Instance);
        
        // Make struct fields accessible directly in method
        foreach (var field in Instance.Fields)
        {
            interpreter.environment.Define(field.Key, field.Value);
        }
        
        // Define parameters
        for (int i = 0; i < Method.Parameters.Count; i++)
        {
            interpreter.environment.Define(Method.Parameters[i].Name.Lexeme, arguments[i]);
        }
        
        try
        {
            interpreter.ExecuteBlock(Method.Body, interpreter.environment);
            
            // Copy modified field values back to the instance
            foreach (var field in Instance.Definition.Fields)
            {
                try
                {
                    object? newValue = interpreter.environment.GetByName(field.Name.Lexeme);
                    Instance.Fields[field.Name.Lexeme] = newValue;
                }
                catch
                {
                    // Field wasn't modified in method, keep original value
                }
            }
        }
        catch (ReturnException returnValue)
        {
            // Copy field changes before returning
            foreach (var field in Instance.Definition.Fields)
            {
                try
                {
                    object? newValue = interpreter.environment.GetByName(field.Name.Lexeme);
                    Instance.Fields[field.Name.Lexeme] = newValue;
                }
                catch
                {
                    // Field wasn't modified in method, keep original value
                }
            }
            // Handle void method returns
            if (Method.ReturnType.Type == TokenType.Void)
            {
                if (returnValue.Value != null)
                {
                    throw new Exception($"Void method '{Method.Name.Lexeme}' cannot return a value");
                }
                return VoidResult.Instance;
            }
            
            return returnValue.Value;
        }
        finally
        {
            interpreter.environment = previous;
        }
        
        // If no return statement, return appropriate default
        return Method.ReturnType.Type == TokenType.Void ? VoidResult.Instance : null;
    }
}