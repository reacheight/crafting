namespace Lox.Lexing;

public readonly union LexingUnit(Token, Comment, Whitespace, UnexpectedCharacter);

public record Comment;
public record Whitespace;
public record UnexpectedCharacter(char Character);