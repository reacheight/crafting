using Lox.Lexing;

namespace Lox.Parsing.Syntax;

public record UnaryOperator(UnaryOperatorType Type, SourceLocation Location);

public readonly union UnaryOperatorType(Not, Negate);

public record struct Not;
public record struct Negate;
