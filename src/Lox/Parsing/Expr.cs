using Lox.Lexing;

namespace Lox.Parsing;

public readonly union Expr(Literal, UnaryExpr, BinaryExpr, Grouping, Ternary, Variable, AssignmentExpr);

public record Literal(LoxValue Value, Token Token);
public record UnaryExpr(UnaryOperator Operator, Expr Expr);
public record BinaryExpr(Expr Left, BinaryOperator Operator, Expr Right);
public record Grouping(Expr Expr);
public record Ternary(Expr Condition, Expr OnTrue, Expr OnFalse);
public record Variable(Token Identifier);
public record AssignmentExpr(Token Target, Expr Value);
