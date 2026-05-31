#pragma warning disable CS8509 // until union exhaustiveness is here

using System.Globalization;

namespace Lox;

public record Token(string Lexeme, int Line, TokenType Type);

public readonly union TokenType(NonLiteralTokenType, LiteralToken)
{
    public override string ToString() => Value switch
    {
        NonLiteralTokenType type => type.ToString(),
        LiteralToken literalToken => literalToken.ToString(),
    };
}

public record LiteralToken(LiteralTokenType Type, TokenLiteral Literal);

public readonly union TokenLiteral(string, double)
{
    public override string ToString() => Value switch
    {
        string s => s,
        double n => n.ToString(CultureInfo.InvariantCulture),
    };
}