using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class Environment(Dictionary<string, LoxValue> globals)
{
    private readonly Stack<Scope> scopes = new([new(globals)]);

    public void DefineVariable(string name, LoxValue value) => CurrentScope.Values[name] = value;

    public void AssignVariable(IdentifierInfo identifier, LoxValue value)
    {
        var containingScope =
            FindContainingScope(identifier.Name)
            ?? throw new RuntimeException(identifier.Location, $"Can't assign undefined variable '{identifier.Name}'.");

        containingScope.Values[identifier.Name] = value;
    }

    public LoxValue GetVariableValue(IdentifierInfo identifier)
    {
        var containingScope =
            FindContainingScope(identifier.Name)
            ?? throw new RuntimeException(identifier.Location, $"Undefined variable '{identifier.Name}'.");

        return containingScope.Values[identifier.Name];
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
