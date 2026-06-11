using Lox.Lexing;

namespace Lox.Parsing;

public readonly union Expr(Literal, UnaryExpr, BinaryExpr, Grouping, Ternary)
{
    public override string ToString() => this switch
    {
        Literal literal => literal.Value.ToString(),
        UnaryExpr unary => $"({unary.Operator.Type} {unary.Expr})",
        BinaryExpr binary => $"({binary.Operator.Type} {binary.Left} {binary.Right})",
        Grouping grouping => $"(group {grouping.Expr})",
        Ternary ternary => $"(ternary {ternary.Condition} ? {ternary.OnTrue} : {ternary.OnFalse})",
    };
}

public record Literal(LoxValue Value, Token Token);
public record UnaryExpr(UnaryOperator Operator, Expr Expr);
public record BinaryExpr(Expr Left, BinaryOperator Operator, Expr Right);
public record Grouping(Expr Expr);
public record Ternary(Expr Condition, Expr OnTrue, Expr OnFalse);
