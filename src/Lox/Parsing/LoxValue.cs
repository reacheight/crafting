#pragma warning disable CS8509 // until union exhaustiveness is not here

using System.Globalization;

namespace Lox.Parsing;

public readonly union LoxValue(string, double, bool, Nil)
{
    public override string ToString() => Value switch
    {
        Nil => "nil",
        string s => s,
        double n => n.ToString(CultureInfo.InvariantCulture),
        bool b => b ? "true" : "false"
    };
}

public record struct Nil;
