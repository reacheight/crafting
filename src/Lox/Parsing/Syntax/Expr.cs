using Lox.Interpreting;
using Lox.Lexing;

namespace Lox.Parsing.Syntax;

public readonly union Expr(Literal, UnaryExpr, BinaryExpr, Grouping, Ternary, Variable, AssignmentExpr, CallExpr, LambdaExpr);

// TODO: extend LiteralToken with bools and nil and use it here
public record Literal(LoxValue Value);
public record UnaryExpr(UnaryOperator Operator, Expr Expr);
public record BinaryExpr(Expr Left, BinaryOperator Operator, Expr Right);
public record Grouping(Expr Expr);
public record Ternary(Expr Condition, Expr OnTrue, Expr OnFalse);
public record Variable(IdentifierInfo Identifier);
public record AssignmentExpr(IdentifierInfo Target, Expr Value);
public record CallExpr(Expr Callee, List<Expr> Arguments, SourceLocation RightParenLocation);
public record LambdaExpr(List<IdentifierInfo> Parameters, List<Stmt> Body);
