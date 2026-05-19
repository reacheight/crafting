namespace Lox;

public record struct Token(TokenType Type, string Lexeme, TokenLiteral? Literal, int Line)
{
    public override string ToString() => $"{Type} {Lexeme} {Literal}";
}

public union TokenLiteral(string, double, bool);
