using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxFunction(FunStmt declaration) : ILoxCallable
{
    private readonly FunStmt declaration = declaration;

    public int Arity => declaration.Parameters.Count;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        var boundParameters = declaration.Parameters.Select(p => p.Name).Zip(arguments).ToDictionary();
        interpreter.ExecuteBlock(new(declaration.Body), boundParameters);
        return new Nil();
    }

    public override string ToString() => $"<fn {declaration.Identifier.Name}>";
}
