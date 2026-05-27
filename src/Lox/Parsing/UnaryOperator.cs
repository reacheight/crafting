#pragma warning disable CS8509 // until union exhaustiveness is not here

namespace Lox.Parsing;

public readonly union UnaryOperator(Not, Negate)
{
    public override string ToString() => Value switch
    {
        Negate => "-",
        Not => "!",
    };
}

public record struct Not;
public record struct Negate;
