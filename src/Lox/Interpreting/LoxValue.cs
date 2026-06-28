using System.Globalization;

namespace Lox.Interpreting;

public readonly union LoxValue(string, double, bool, ILoxCallable, Nil)
{
    public override string ToString() => this switch
    {
        Nil => "nil",
        string s => s,
        double n => n.ToString(CultureInfo.InvariantCulture),
        bool b => b ? "true" : "false",
        ILoxCallable => "some callable",
    };

    public override bool Equals(object? obj)
    {
        if (obj is not LoxValue value)
            return false;

        return value switch
        {
            Nil => this is Nil,
            string otherString => this is string thisString && thisString.Equals(otherString),
            double otherNumber => this is double thisNumber && thisNumber.Equals(otherNumber),
            bool otherBool => this is bool thisBool && thisBool.Equals(otherBool),
            ILoxCallable => false,
        };
    }

    public override int GetHashCode() => this switch
    {
        Nil => 0,
        string s => s.GetHashCode(),
        double n => n.GetHashCode(),
        bool b => b.GetHashCode(),
        ILoxCallable c => c.GetHashCode(),
    };

    public static bool operator ==(LoxValue left, LoxValue right) => left.Equals(right);
    public static bool operator !=(LoxValue left, LoxValue right) => !left.Equals(right);
}

public record struct Nil;
