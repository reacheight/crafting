#pragma warning disable RCS1194

namespace Lox.Parsing;

public class ParseException(Token token, string message) : Exception(message)
{
    public Token Token { get; init; } = token;
}
