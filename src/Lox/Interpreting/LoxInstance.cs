using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxInstance(LoxClass @class)
{
    private readonly Dictionary<string, LoxValue> fields = [];

    public LoxValue Get(IdentifierInfo propertyId)
        => fields.TryGetValue(propertyId.Name, out var value)
            ? value
            : throw new RuntimeException(propertyId.Location, $"Undefined property {propertyId.Name}.");

    public LoxValue Set(IdentifierInfo propertyId, LoxValue value)
        => fields[propertyId.Name] = value;

    public override string ToString() => $"{@class.Name} instance";
}
