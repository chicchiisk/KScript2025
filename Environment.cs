namespace Calculator;

public class Environment
{
    private readonly Dictionary<string, object?> values = new();
    private readonly Environment? enclosing;

    public Environment()
    {
        enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }

    public void Define(string name, object? value)
    {
        values[name] = value;
    }

    public object? Get(Token name)
    {
        if (values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        if (enclosing != null) return enclosing.Get(name);

        throw new Exception($"Undefined variable '{name.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new Exception($"Undefined variable '{name.Lexeme}'.");
    }

    public bool IsDefined(string name)
    {
        if (values.ContainsKey(name)) return true;
        if (enclosing != null) return enclosing.IsDefined(name);
        return false;
    }

    public object? GetByName(string name)
    {
        if (values.TryGetValue(name, out var value))
        {
            return value;
        }

        if (enclosing != null) return enclosing.GetByName(name);

        throw new Exception($"Undefined variable '{name}'.");
    }
}