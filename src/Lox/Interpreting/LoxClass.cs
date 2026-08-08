namespace Lox.Interpreting;

public class LoxClass(string name, Dictionary<string, LoxFunction> methods, LoxClass? superclass) : ILoxCallable
{
    public string Name => name;

    public int Arity => FindMethod("init")?.Arity ?? 0;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        var instance = new LoxInstance(this);
        return FindMethod("init")?.Bind(instance).Call(interpreter, arguments) ?? instance;
    }

    public LoxFunction? FindMethod(string name)
        => methods.TryGetValue(name, out var method)
            ? method
            : superclass?.FindMethod(name);

    public override string ToString() => Name;
}
