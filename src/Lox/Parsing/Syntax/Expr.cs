using Lox.Interpreting;
using Lox.Lexing;

namespace Lox.Parsing.Syntax;

public readonly union Expr(
    Literal,
    UnaryExpr,
    BinaryExpr,
    Grouping,
    Ternary,
    Variable,
    AssignmentExpr,
    CallExpr,
    LambdaExpr,
    GetExpr,
    SetExpr,
    ThisExpr,
    SuperExpr,
    IsExpr
);

public class Literal(LoxValue value)
{
    public LoxValue Value { get; } = value;
}

public class UnaryExpr(UnaryOperator @operator, Expr expr)
{
    public UnaryOperator Operator { get; } = @operator;
    public Expr Expr { get; } = expr;
}

public class BinaryExpr(Expr left, BinaryOperator @operator, Expr right)
{
    public Expr Left { get; } = left;
    public BinaryOperator Operator { get; } = @operator;
    public Expr Right { get; } = right;
}

public class Grouping(Expr expr)
{
    public Expr Expr { get; } = expr;
}

public class Ternary(Expr condition, Expr onTrue, Expr onFalse)
{
    public Expr Condition { get; } = condition;
    public Expr OnTrue { get; } = onTrue;
    public Expr OnFalse { get; } = onFalse;
}

public class Variable(IdentifierInfo identifier)
{
    public IdentifierInfo Identifier { get; } = identifier;
}

public class AssignmentExpr(IdentifierInfo target, Expr value)
{
    public IdentifierInfo Target { get; } = target;
    public Expr Value { get; } = value;
}

public class CallExpr(Expr callee, List<Expr> arguments, SourceLocation rightParenLocation)
{
    public Expr Callee { get; } = callee;
    public List<Expr> Arguments { get; } = arguments;
    public SourceLocation RightParenLocation { get; } = rightParenLocation;
}

public class LambdaExpr(List<IdentifierInfo> parameters, List<Stmt> body)
{
    public List<IdentifierInfo> Parameters { get; } = parameters;
    public List<Stmt> Body { get; } = body;
}

public class GetExpr(Expr instance, IdentifierInfo name)
{
    public Expr Instance { get; } = instance;
    public IdentifierInfo Name { get; } = name;
}

public class SetExpr(Expr instance, IdentifierInfo name, Expr value)
{
    public Expr Instance { get; } = instance;
    public IdentifierInfo Name { get; } = name;
    public Expr Value { get; } = value;
}

public class ThisExpr(SourceLocation location)
{
    public SourceLocation Location { get; } = location;
}

public class SuperExpr(SourceLocation keywordLocation, IdentifierInfo method)
{
    public SourceLocation KeywordLocation { get; } = keywordLocation;
    public IdentifierInfo Method { get; } = method;
}

public class IsExpr(Expr expr, Pattern pattern, SourceLocation patternLocation)
{
    public Expr Expr { get; } = expr;
    public Pattern Pattern { get; } = pattern;
    public SourceLocation PatternLocation { get; } = patternLocation;
}

public readonly union Pattern(Literal, Variable, StrPattern, NumPattern, BoolPattern);
public record struct StrPattern;
public record struct NumPattern;
public record struct BoolPattern;
