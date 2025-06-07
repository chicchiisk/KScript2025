namespace Calculator.Runtime;

/// <summary>
/// Handles array operations
/// </summary>
public static class ArrayOperations
{
    public static object? Access(Interpreter interpreter, ArrayAccessExpr expr)
    {
        object? arrayObj = interpreter.Evaluate(expr.Array);
        
        if (arrayObj is not Array array)
        {
            throw new Exception("Cannot index non-array value");
        }

        List<int> indices = new();
        foreach (Expr indexExpr in expr.Indices)
        {
            object? indexObj = interpreter.Evaluate(indexExpr);
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

    public static object? Assign(Interpreter interpreter, ArrayAssignExpr expr)
    {
        object? value = interpreter.Evaluate(expr.Value);
        object? arrayObj = interpreter.Evaluate(expr.Target.Array);
        
        if (arrayObj is not Array array)
        {
            throw new Exception("Cannot index non-array value");
        }

        List<int> indices = new();
        foreach (Expr indexExpr in expr.Target.Indices)
        {
            object? indexObj = interpreter.Evaluate(indexExpr);
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
            "Int32" => TypeConverter.ConvertToInt(value, null),
            "Single" => TypeConverter.ConvertToFloat(value, null),
            "Char" => TypeConverter.ConvertToChar(value, null),
            "Boolean" => TypeConverter.ConvertToBool(value, null),
            "Object" => value, // For struct arrays (object[])
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

    public static object? CreateLiteral(Interpreter interpreter, ArrayLiteralExpr expr)
    {
        if (expr.Elements.Count == 0)
        {
            // Empty array - default to int[]
            return new int[0];
        }

        List<object?> values = new();
        foreach (Expr element in expr.Elements)
        {
            object? value = interpreter.Evaluate(element);
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

    public static object? CreateNew(Interpreter interpreter, ArrayNewExpr expr)
    {
        List<int> dimensions = new();
        foreach (Expr dimExpr in expr.Dimensions)
        {
            object? dimObj = interpreter.Evaluate(dimExpr);
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
            (TokenType.Identifier, 1) => new object?[dimensions[0]], // Struct arrays
            (TokenType.Identifier, 2) => new object?[dimensions[0], dimensions[1]],
            (TokenType.Identifier, 3) => new object?[dimensions[0], dimensions[1], dimensions[2]],
            _ => throw new Exception($"Arrays with {dimensions.Count} dimensions not supported for type {expr.Type.Type}")
        };
    }

    #region Array Creation Helpers

    private static int[] Create1DIntArray(List<object?> values)
    {
        int[] result = new int[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (int)TypeConverter.ConvertToInt(values[i], null);
        }
        return result;
    }

    private static float[] Create1DFloatArray(List<object?> values)
    {
        float[] result = new float[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (float)TypeConverter.ConvertToFloat(values[i], null);
        }
        return result;
    }

    private static char[] Create1DCharArray(List<object?> values)
    {
        char[] result = new char[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (char)TypeConverter.ConvertToChar(values[i], null);
        }
        return result;
    }

    private static bool[] Create1DBoolArray(List<object?> values)
    {
        bool[] result = new bool[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            result[i] = (bool)TypeConverter.ConvertToBool(values[i], null);
        }
        return result;
    }

    private static int[,] Create2DIntArray(List<object?> values, int rows, int cols)
    {
        int[,] result = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (int)TypeConverter.ConvertToInt(row.GetValue(j), null);
            }
        }
        return result;
    }

    private static float[,] Create2DFloatArray(List<object?> values, int rows, int cols)
    {
        float[,] result = new float[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (float)TypeConverter.ConvertToFloat(row.GetValue(j), null);
            }
        }
        return result;
    }

    private static char[,] Create2DCharArray(List<object?> values, int rows, int cols)
    {
        char[,] result = new char[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (char)TypeConverter.ConvertToChar(row.GetValue(j), null);
            }
        }
        return result;
    }

    private static bool[,] Create2DBoolArray(List<object?> values, int rows, int cols)
    {
        bool[,] result = new bool[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            Array row = (Array)values[i]!;
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = (bool)TypeConverter.ConvertToBool(row.GetValue(j), null);
            }
        }
        return result;
    }

    #endregion
}