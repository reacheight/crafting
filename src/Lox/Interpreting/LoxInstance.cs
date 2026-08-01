namespace Lox.Interpreting;

public class LoxInstance(LoxClass @class)
{
    public override string ToString() => $"{@class.Name} instance";
}
