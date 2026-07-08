using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class Environment(Dictionary<string, LoxValue> globals)
{
    private readonly Scope globalScope = new(globals);
    private readonly Stack<Scope> innerScopes = new();

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

    public void EnterScope() => innerScopes.Push(new([]));
    public void ExitScope() => innerScopes.Pop();

    public Dictionary<string, LoxValue> GetGlobals() => globalScope.Values;

    private Scope CurrentScope => innerScopes.Count > 0 ? innerScopes.Peek() : globalScope;

    private Scope? FindContainingScope(string name)
        => innerScopes.FirstOrDefault(scope => scope.Values.ContainsKey(name))
            ?? (globalScope.Values.ContainsKey(name) ? globalScope : null);

    private record Scope(Dictionary<string, LoxValue> Values);
}
