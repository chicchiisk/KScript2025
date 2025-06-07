namespace Calculator.Runtime;

/// <summary>
/// Represents a string instance as a reference type
/// Strings are immutable reference types in this language
/// </summary>
public class StringInstance : ObjectBase
{
    public string Value { get; }

    public StringInstance(string value, int heapId) : base(heapId)
    {
        Value = value ?? "";
    }

    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Creates a new string instance on the heap
    /// </summary>
    public static StringInstance Create(string value, HeapManager heapManager)
    {
        // For simplicity, allocate heap space first then create the instance
        var tempObject = new ObjectBase(0);
        int heapId = heapManager.Allocate(tempObject);
        
        // Create the string instance with the correct heap ID
        return new StringInstance(value, heapId);
    }
}