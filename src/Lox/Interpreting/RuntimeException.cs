#pragma warning disable RCS1194

using Lox.Lexing;

namespace Lox.Interpreting;

public class RuntimeException(SourceLocation location, string message) : Exception(message)
{
    public SourceLocation Location { get; init; } = location;
}
