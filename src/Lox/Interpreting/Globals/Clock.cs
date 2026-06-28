namespace Lox.Interpreting.Globals;

public class Clock : ILoxCallable
{
    public int Arity => 0;

    public LoxValue Call(Interpreter interpreter, List<LoxValue> arguments)
        => DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000;

    public override string ToString() => "<native fn>";
}
