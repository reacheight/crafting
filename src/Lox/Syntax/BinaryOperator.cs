#pragma warning disable CS8509 // until union exhaustiveness is not here

namespace Lox.Syntax;

public readonly union BinaryOperator(
    Plus, Minus, Star, Slash,
    Less, LessEqual, Greater, GreaterEqual,
    EqualEqual, BangEqual
)
{
    public override string ToString() => Value switch
    {
        Plus => "+",
        Minus => "-",
        Star => "*",
        Slash => "/",
        Less => "<",
        LessEqual => "<=",
        Greater => ">",
        GreaterEqual => ">=",
        EqualEqual => "==",
        BangEqual => "!=",
    };
}

public record struct Minus;
public record struct Plus;
public record struct Star;
public record struct Slash;
public record struct Less;
public record struct LessEqual;
public record struct Greater;
public record struct GreaterEqual;
public record struct EqualEqual;
public record struct BangEqual;
