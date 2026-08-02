using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class Environment(Environment? parent = null)
{
    private readonly Dictionary<string, LoxValue> values = [];

    public void Define(string name, LoxValue value) => values[name] = value;

    public LoxValue Get(IdentifierInfo id)
    {
        if (values.TryGetValue(id.Name, out var val))
            return val;

        return parent?.Get(id)
            ?? throw new RuntimeException(id.Location, $"Undefined variable '{id.Name}'.");
    }

    public LoxValue GetAt(int depth, string name)
        => depth is 0
            ? values[name]
            : parent!.GetAt(depth - 1, name);

    public void Assign(IdentifierInfo id, LoxValue value)
    {
        if (values.ContainsKey(id.Name))
        {
            values[id.Name] = value;
            return;
        }

        if (parent is not null)
        {
            parent.Assign(id, value);
            return;
        }

        throw new RuntimeException(id.Location, $"Can't assign undefined variable '{id.Name}'.");
    }

    public void AssignAt(int depth, IdentifierInfo id, LoxValue value)
    {
        if (depth is 0)
            values[id.Name] = value;
        else
            parent!.AssignAt(depth - 1, id, value);
    }
}
