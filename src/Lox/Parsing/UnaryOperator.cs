namespace Lox.Parsing;

public readonly union UnaryOperator(Not, Negate)
{
    public override string ToString() => this switch
    {
        Negate => "-",
        Not => "!",
    };
}

public record struct Not;
public record struct Negate;
