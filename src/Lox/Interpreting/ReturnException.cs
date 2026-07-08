#pragma warning disable RCS1194

namespace Lox.Interpreting;

public class ReturnException(LoxValue value) : Exception
{
    public LoxValue Value { get; } = value;
}
