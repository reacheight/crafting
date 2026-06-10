using System.Globalization;

namespace Lox.Parsing;

public readonly union LoxValue(string, double, bool, Nil)
{
    public override string ToString() => this switch
    {
        Nil => "nil",
        string s => s,
        double n => n.ToString(CultureInfo.InvariantCulture),
        bool b => b ? "true" : "false"
    };
}

public record struct Nil;
