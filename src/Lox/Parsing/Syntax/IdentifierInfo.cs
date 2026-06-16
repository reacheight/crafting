using Lox.Lexing;

namespace Lox.Parsing.Syntax;

public record IdentifierInfo(string Name, SourceLocation Location);
