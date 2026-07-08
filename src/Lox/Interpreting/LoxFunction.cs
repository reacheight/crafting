using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxFunction(FunDecl declaration) : ILoxCallable
{
    private readonly FunDecl declaration = declaration;

    public int Arity => declaration.Parameters.Count;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        var boundParameters = declaration.Parameters.Select(p => p.Name).Zip(arguments).ToDictionary();

        try
        {
            interpreter.ExecuteBlock(new(declaration.Body), boundParameters);
        }
        catch (ReturnException ret)
        {
            return ret.Value;
        }

        return new Nil();
    }

    public override string ToString() => $"<fn {declaration.Identifier.Name}>";
}
