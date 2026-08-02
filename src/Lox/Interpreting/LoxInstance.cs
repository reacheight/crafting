using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxInstance(LoxClass @class)
{
    private readonly Dictionary<string, LoxValue> fields = [];

    public LoxValue Get(IdentifierInfo propertyId)
    {
        if (fields.TryGetValue(propertyId.Name, out var field))
            return field;

        return @class.GetMethod(propertyId.Name)?.Bind(this)
            ?? throw new RuntimeException(propertyId.Location, $"Undefined property {propertyId.Name}.");
    }

    public LoxValue Set(IdentifierInfo propertyId, LoxValue value)
        => fields[propertyId.Name] = value;

    public override string ToString() => $"{@class.Name} instance";
}
