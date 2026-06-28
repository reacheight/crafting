namespace Lox.Interpreting;

public interface ILoxCallable
{
    public int Arity { get; }

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments);
}
