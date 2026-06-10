namespace Lox.Parsing;

public readonly union BinaryOperator(
    Add, Substract, Multiply, Divide,
    Less, LessEqual, Greater, GreaterEqual,
    Equal, NotEqual
)
{
    public override string ToString() => this switch
    {
        Add => "+",
        Substract => "-",
        Multiply => "*",
        Divide => "/",
        Less => "<",
        LessEqual => "<=",
        Greater => ">",
        GreaterEqual => ">=",
        Equal => "==",
        NotEqual => "!=",
    };
}

public record struct Substract;
public record struct Add;
public record struct Multiply;
public record struct Divide;
public record struct Less;
public record struct LessEqual;
public record struct Greater;
public record struct GreaterEqual;
public record struct Equal;
public record struct NotEqual;
