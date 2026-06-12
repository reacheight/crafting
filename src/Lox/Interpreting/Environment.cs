using Lox.Lexing;
using Lox.Parsing;

namespace Lox.Interpreting;

public class Environment
{
    private readonly Dictionary<string, LoxValue> values = [];

    public void DefineVariable(string name, LoxValue value) => values[name] = value;

    public LoxValue GetVariableValue(Token identifier)
        => values.TryGetValue(identifier.Lexeme, out var value)
            ? value
            : throw new RuntimeException(identifier, $"Undefined variable '{identifier.Lexeme}'.");
}
