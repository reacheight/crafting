#pragma warning disable CS8509 // until union exhaustiveness is not here

namespace Lox.Syntax;

public readonly union UnaryOperator(Bang, Negate)
{
    public override string ToString() => Value switch
    {
        Negate => "-",
        Bang => "!",
    };
}

public record struct Bang;
public record struct Negate;
