namespace Calculator.Runtime;

/// <summary>
/// Handles type conversions for the scripting language
/// </summary>
public static class TypeConverter
{
    /// <summary>
    /// Converts a value to int with optional default
    /// </summary>
    public static object ConvertToInt(object? value, int? defaultValue)
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

    /// <summary>
    /// Converts a value to float with optional default
    /// </summary>
    public static object ConvertToFloat(object? value, float? defaultValue)
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

    /// <summary>
    /// Converts a value to char with optional default
    /// </summary>
    public static object ConvertToChar(object? value, char? defaultValue)
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

    /// <summary>
    /// Converts a value to bool with optional default
    /// </summary>
    public static object ConvertToBool(object? value, bool? defaultValue)
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

    /// <summary>
    /// Converts a value to double for arithmetic operations
    /// </summary>
    public static double ConvertToDouble(object? value)
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
}