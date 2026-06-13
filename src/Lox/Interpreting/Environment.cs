using Lox.Lexing;
using Lox.Parsing;

namespace Lox.Interpreting;

public class Environment
{
    private readonly Stack<Scope> scopes = new([new()]);

    public void DefineVariable(string name, LoxValue value) => CurrentScope.Values[name] = value;

    public void AssignVariable(Token identifier, LoxValue value)
    {
        var containingScope =
            FindContainingScope(identifier.Lexeme)
            ?? throw new RuntimeException(identifier, $"Can't assign undefined variable '{identifier.Lexeme}'.");

        containingScope.Values[identifier.Lexeme] = value;
    }

    public LoxValue GetVariableValue(Token identifier)
    {
        var containingScope =
            FindContainingScope(identifier.Lexeme)
            ?? throw new RuntimeException(identifier, $"Undefined variable '{identifier.Lexeme}'.");

        return containingScope.Values[identifier.Lexeme];
    }

    public void EnterScope() => scopes.Push(new());
    public void ExitScope() => scopes.Pop();

    private Scope CurrentScope => scopes.Peek();
    private Scope? FindContainingScope(string name) => scopes.FirstOrDefault(scope => scope.Values.ContainsKey(name));

    private record Scope(Dictionary<string, LoxValue> Values)
    {
        public Scope() : this([]) { }
    }
}
