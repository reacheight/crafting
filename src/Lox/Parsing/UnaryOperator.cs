using Lox.Lexing;

namespace Lox.Parsing;

public record UnaryOperator(UnaryOperatorType Type, Token Token);

public readonly union UnaryOperatorType(Not, Negate)
{
    public override string ToString() => this switch
    {
        Negate => "-",
        Not => "!",
    };
}

public record struct Not;
public record struct Negate;
