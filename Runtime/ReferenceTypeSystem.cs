namespace Calculator.Runtime;

/// <summary>
/// Base object type - all reference types implicitly inherit from this
/// </summary>
public class ObjectBase
{
    public int HeapId { get; }
    
    public ObjectBase(int heapId)
    {
        HeapId = heapId;
    }
    
    public override string ToString()
    {
        return GetType().Name;
    }
}

/// <summary>
/// Definition of a class type with its structure
/// </summary>
public class ClassDefinition
{
    public string Name { get; }
    public Dictionary<string, object?> DefaultFields { get; }
    public Dictionary<string, Function> Methods { get; }
    public Function? Constructor { get; private set; }

    public ClassDefinition(string name)
    {
        Name = name;
        DefaultFields = new Dictionary<string, object?>();
        Methods = new Dictionary<string, Function>();
    }

    public void AddField(string name, object? defaultValue)
    {
        DefaultFields[name] = defaultValue;
    }

    public void AddMethod(string name, Function method)
    {
        Methods[name] = method;
    }

    public void SetConstructor(Function constructor)
    {
        Constructor = constructor;
    }
}

/// <summary>
/// Instance of a class allocated on the heap
/// </summary>
public class ClassInstance : ObjectBase
{
    public ClassDefinition Definition { get; }
    public Dictionary<string, object?> Fields { get; }

    public ClassInstance(ClassDefinition definition, int heapId) : base(heapId)
    {
        Definition = definition;
        Fields = new Dictionary<string, object?>();
        
        // Initialize fields with default values
        foreach (var field in definition.DefaultFields)
        {
            Fields[field.Key] = field.Value;
        }
    }

    public object? GetField(string name)
    {
        if (Fields.TryGetValue(name, out var value))
        {
            return value;
        }
        throw new Exception($"Undefined field '{name}' on class '{Definition.Name}'.");
    }

    public void SetField(string name, object? value)
    {
        if (Definition.DefaultFields.ContainsKey(name))
        {
            Fields[name] = value;
        }
        else
        {
            throw new Exception($"Undefined field '{name}' on class '{Definition.Name}'.");
        }
    }

    public Function? GetMethod(string name)
    {
        if (Definition.Methods.TryGetValue(name, out var method))
        {
            return method;
        }
        return null;
    }
    
    public override string ToString()
    {
        return $"{Definition.Name}";
    }
}

/// <summary>
/// Bound method that carries the class instance with it
/// </summary>
public class BoundClassMethod : ICallable
{
    private readonly ClassInstance instance;
    private readonly Function method;

    public BoundClassMethod(ClassInstance instance, Function method)
    {
        this.instance = instance;
        this.method = method;
    }

    public int Arity => method.Arity;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        // Create new environment with 'this' bound to the instance
        Environment previous = interpreter.environment;
        
        try
        {
            interpreter.environment = new Environment(method.Closure);
            interpreter.environment.Define("this", instance);
            
            // Make class fields accessible directly in method
            foreach (var field in instance.Fields)
            {
                interpreter.environment.Define(field.Key, field.Value);
            }
            
            // Bind parameters  
            for (int i = 0; i < method.Declaration.Parameters.Count; i++)
            {
                if (i < arguments.Count)
                {
                    interpreter.environment.Define(method.Declaration.Parameters[i].Name.Lexeme, arguments[i]);
                }
            }
            
            // Execute method body
            try
            {
                foreach (Stmt stmt in method.Declaration.Body)
                {
                    interpreter.Execute(stmt);
                }
            }
            catch (ReturnException returnValue)
            {
                // Copy modified field values back to the instance before returning
                foreach (var fieldName in instance.Definition.DefaultFields.Keys)
                {
                    try
                    {
                        object? newValue = interpreter.environment.GetByName(fieldName);
                        instance.SetField(fieldName, newValue);
                    }
                    catch (Exception)
                    {
                        // Field wasn't modified in method, keep existing value
                    }
                }
                
                // Handle void method returns
                if (method.Declaration.ReturnType.Type == TokenType.Void)
                {
                    if (returnValue.Value != null)
                    {
                        throw new Exception($"Void method '{method.Declaration.Name.Lexeme}' cannot return a value");
                    }
                    return VoidResult.Instance;
                }
                
                return returnValue.Value;
            }
            
            // Copy modified field values back to the instance
            foreach (var fieldName in instance.Definition.DefaultFields.Keys)
            {
                try
                {
                    object? newValue = interpreter.environment.GetByName(fieldName);
                    instance.SetField(fieldName, newValue);
                }
                catch (Exception)
                {
                    // Field wasn't modified in method, keep existing value
                }
            }
            
            // If no return statement, return appropriate default
            return method.Declaration.ReturnType.Type == TokenType.Void ? VoidResult.Instance : null;
        }
        finally
        {
            interpreter.environment = previous;
        }
    }

    public override string ToString()
    {
        return $"<bound method {method.Declaration.Name.Lexeme}>";
    }
}

/// <summary>
/// Simple heap manager with garbage collection
/// </summary>
public class HeapManager
{
    private readonly Dictionary<int, ObjectBase> heap;
    private readonly HashSet<int> roots;
    private int nextId;

    public HeapManager()
    {
        heap = new Dictionary<int, ObjectBase>();
        roots = new HashSet<int>();
        nextId = 1;
    }

    /// <summary>
    /// Allocates an object on the heap and returns its ID
    /// </summary>
    public int Allocate(ObjectBase obj)
    {
        int id = nextId++;
        heap[id] = obj;
        roots.Add(id); // Initially all objects are roots
        return id;
    }

    /// <summary>
    /// Gets an object from the heap
    /// </summary>
    public ObjectBase? GetObject(int id)
    {
        return heap.TryGetValue(id, out var obj) ? obj : null;
    }

    /// <summary>
    /// Marks an object as a root (prevents GC)
    /// </summary>
    public void AddRoot(int id)
    {
        roots.Add(id);
    }

    /// <summary>
    /// Removes an object from roots (allows GC)
    /// </summary>
    public void RemoveRoot(int id)
    {
        roots.Remove(id);
    }

    /// <summary>
    /// Simple garbage collection - removes non-root objects
    /// For simplicity, we'll use a basic mark-and-sweep approach
    /// </summary>
    public void Collect()
    {
        var reachable = new HashSet<int>();
        
        // Mark phase: start from roots
        foreach (var rootId in roots)
        {
            MarkReachable(rootId, reachable);
        }

        // Sweep phase: remove unreachable objects
        var toRemove = heap.Keys.Where(id => !reachable.Contains(id)).ToList();
        foreach (var id in toRemove)
        {
            heap.Remove(id);
        }
    }

    private void MarkReachable(int id, HashSet<int> reachable)
    {
        if (reachable.Contains(id) || !heap.ContainsKey(id))
            return;

        reachable.Add(id);

        // Mark objects referenced by this object
        var obj = heap[id];
        if (obj is ClassInstance classInstance)
        {
            // Mark objects referenced in fields
            foreach (var field in classInstance.Fields.Values)
            {
                if (field is ClassInstance referencedInstance)
                {
                    MarkReachable(referencedInstance.HeapId, reachable);
                }
            }
        }
    }

    public int Count => heap.Count;
}