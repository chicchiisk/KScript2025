namespace Calculator.Runtime;

/// <summary>
/// Handles arithmetic operations with proper type handling
/// </summary>
public static class ArithmeticOperations
{
    public static object Add(object? left, object? right)
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
        double leftDouble = TypeConverter.ConvertToDouble(left);
        double rightDouble = TypeConverter.ConvertToDouble(right);
        return leftDouble + rightDouble;
    }

    public static object Subtract(object? left, object? right)
    {
        if (left is int leftInt && right is int rightInt)
            return leftInt - rightInt;
        
        if (left is char leftChar && right is int rightIntForChar)
            return (char)(leftChar - rightIntForChar);
        
        if (left is char leftChar2 && right is char rightChar)
            return leftChar2 - rightChar;
        
        if ((left is float || right is float) && 
            (left is int || left is float) && (right is int || right is float))
        {
            float leftFloat = left is float lf ? lf : (int)left!;
            float rightFloat = right is float rf ? rf : (int)right!;
            return leftFloat - rightFloat;
        }

        double leftDouble = TypeConverter.ConvertToDouble(left);
        double rightDouble = TypeConverter.ConvertToDouble(right);
        return leftDouble - rightDouble;
    }

    public static object Multiply(object? left, object? right)
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

        double leftDouble = TypeConverter.ConvertToDouble(left);
        double rightDouble = TypeConverter.ConvertToDouble(right);
        return leftDouble * rightDouble;
    }

    public static object Divide(object? left, object? right)
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

        double leftDouble = TypeConverter.ConvertToDouble(left);
        double rightDouble = TypeConverter.ConvertToDouble(right);
        return rightDouble != 0 ? leftDouble / rightDouble : throw new Exception("Division by zero");
    }

    public static object Modulo(object? left, object? right)
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

        double leftDouble = TypeConverter.ConvertToDouble(left);
        double rightDouble = TypeConverter.ConvertToDouble(right);
        return rightDouble != 0 ? leftDouble % rightDouble : throw new Exception("Modulo by zero");
    }
}