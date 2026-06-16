using Lox.Lexing;

namespace Lox.Parsing.Syntax;

public record BinaryOperator(BinaryOperatorType Type, SourceLocation Location);

public readonly union BinaryOperatorType(
    Add, Substract, Multiply, Divide,
    Less, LessEqual, Greater, GreaterEqual,
    Equal, NotEqual, LogicalAnd, LogicalOr
);

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
public record struct LogicalAnd;
public record struct LogicalOr;
