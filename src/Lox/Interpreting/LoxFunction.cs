using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxFunction(string? name, List<IdentifierInfo> parameters, List<Stmt> body, Environment closure, bool isInit) : ILoxCallable
{
    public int Arity => parameters.Count;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        var env = new Environment(closure);

        var boundParameters = parameters.Select(p => p.Name).Zip(arguments);
        foreach (var (name, val) in boundParameters)
            env.Define(name, val);

        try
        {
            interpreter.ExecuteBlock(body, env);
        }
        catch (ReturnException ret)
        {
            return isInit ? closure.GetAt(0, "this") : ret.Value;
        }

        return isInit ? closure.GetAt(0, "this") : new Nil();
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        var methodEnv = new Environment(closure);
        methodEnv.Define("this", instance);
        return new LoxFunction(name, parameters, body, methodEnv, isInit);
    }

    public override string ToString() => name is null ? "<lambda>" : $"<fn {name}>";
}
