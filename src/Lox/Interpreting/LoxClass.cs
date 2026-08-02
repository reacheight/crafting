using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxClass(ClassDecl classDecl, Dictionary<string, LoxFunction> methods) : ILoxCallable
{
    public string Name => classDecl.Identifier.Name;

    public int Arity => 0;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        return new LoxInstance(this);
    }

    public LoxFunction? GetMethod(string name)
        => methods.TryGetValue(name, out var method) ? method : null;

    public override string ToString() => Name;
}
