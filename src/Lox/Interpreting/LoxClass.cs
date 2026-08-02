using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxClass(ClassDecl classDecl, Dictionary<string, LoxFunction> methods) : ILoxCallable
{
    public string Name => classDecl.Identifier.Name;

    public int Arity => FindMethod("init")?.Arity ?? 0;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        var instance = new LoxInstance(this);
        return FindMethod("init")?.Bind(instance).Call(interpreter, arguments) ?? instance;
    }

    public LoxFunction? FindMethod(string name)
        => methods.TryGetValue(name, out var method) ? method : null;

    public override string ToString() => Name;
}
