namespace Lox.Lexing;

public readonly union LexingUnit(Token, Comment, Whitespace, SyntaxError);

public record Comment;
public record Whitespace;
public record SyntaxError(string Message);