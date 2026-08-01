using Lox.Parsing.Syntax;

namespace Lox.Interpreting;

public class LoxClass(ClassDecl classDecl) : ILoxCallable
{
    public string Name => classDecl.Identifier.Name;

    public int Arity => 0;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
    {
        return new LoxInstance(this);
    }

    public override string ToString() => Name;
}
