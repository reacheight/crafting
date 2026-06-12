using Lox.Lexing;

namespace Lox.Parsing;

public readonly union ParseResult(LoxProgram, ParseError);

public record LoxProgram(List<Stmt> Statements);
public record ParseError(Token Token, string Message);
