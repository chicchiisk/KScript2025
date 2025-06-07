namespace Calculator.Runtime;

/// <summary>
/// Special marker class to represent the result of a void function call
/// This helps distinguish between null values and void returns
/// </summary>
public class VoidResult
{
    public static readonly VoidResult Instance = new VoidResult();
    
    private VoidResult() { }
    
    public override string ToString()
    {
        return "void";
    }
}