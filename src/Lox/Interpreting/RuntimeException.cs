#pragma warning disable RCS1194

using Lox.Lexing;

namespace Lox.Interpreting;

public class RuntimeException(Token token, string message) : Exception(message)
{
    public Token Token { get; init; } = token;
}
