namespace Lox.Lexing;

public readonly union LexingUnit(Token, Comment, Whitespace, SyntaxError);

public record struct Comment;
public record struct Whitespace;
public record SyntaxError(string Message);