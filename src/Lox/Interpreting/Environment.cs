using Lox.Lexing;
using Lox.Parsing;

namespace Lox.Interpreting;

public class Environment
{
    private readonly Dictionary<string, LoxValue> values = [];

    public void DefineVariable(string name, LoxValue value) => values[name] = value;

    public void AssignVariable(Token identifier, LoxValue value)
    {
        if (values.ContainsKey(identifier.Lexeme))
            values[identifier.Lexeme] = value;
        else
            throw new RuntimeException(identifier, $"Can't assign undefined variable '{identifier.Lexeme}'.");
    }

    public LoxValue GetVariableValue(Token identifier)
        => values.TryGetValue(identifier.Lexeme, out var value)
            ? value
            : throw new RuntimeException(identifier, $"Undefined variable '{identifier.Lexeme}'.");
}
