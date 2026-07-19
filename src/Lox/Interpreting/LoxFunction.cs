using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxFunction(FunDecl declaration, Environment closure) : ILoxCallable
{
    public int Arity => declaration.Parameters.Count;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        var env = new Environment(closure);

        var boundParameters = declaration.Parameters.Select(p => p.Name).Zip(arguments);
        foreach (var (name, val) in boundParameters)
            env.Define(name, val);

        try
        {
            interpreter.ExecuteBlock(declaration.Body, env);
        }
        catch (ReturnException ret)
        {
            return ret.Value;
        }

        return new Nil();
    }

    public override string ToString() => $"<fn {declaration.Identifier.Name}>";
}
