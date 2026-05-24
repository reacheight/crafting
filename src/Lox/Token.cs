namespace Lox;

public record Token(TokenType Type, string Lexeme, TokenLiteral? Literal, int Line)
{
    public override string ToString() => $"{Type} {Lexeme} {Literal?.Value}";
}

public readonly union TokenLiteral(string, double);