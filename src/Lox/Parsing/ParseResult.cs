using Lox.Lexing;

namespace Lox.Parsing;

public readonly union ParseResult(Expr, ParseError);

public record ParseError(Token Token, string Message);
