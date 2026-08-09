using Lox.Interpreting;
using Lox.Lexing;
using Lox.Parsing.Syntax;
using Lox.Utils;

namespace Lox.Resolving;

public class Resolver(Interpreter interpreter)
{
    private readonly Stack<Dictionary<string, NameState>> scopes = [];

    public void Resolve(List<Stmt> statements)
    {
        foreach (var stmt in statements)
            Resolve(stmt);
    }

    private Unit Resolve(Stmt stmt) => stmt switch
    {
        Block block => ResolveBlock(block),
        VarDecl varDecl => ResolveVarDecl(varDecl),
        FunDecl funDecl => ResolveFunDecl(funDecl),
        ExprStmt exprStmt => Resolve(exprStmt.Expr),
        PrintStmt printStmt => Resolve(printStmt.Expr),
        ReturnStmt returnStmt when returnStmt.Expr.HasValue => Resolve(returnStmt.Expr.Value),
        IfStmt ifStmt => ResolveIfStmt(ifStmt),
        WhileStmt whileStmt => ResolveWhileStmt(whileStmt),
        ClassDecl classDecl => ResolveClassDecl(classDecl),

        _ => new(),
    };

    private Unit ResolveClassDecl(ClassDecl classDecl)
    {
        Declare(classDecl.Identifier.Name, new DeclaredName(classDecl.Identifier.Location));

        if (classDecl.Superclass is { } superclass)
        {
            Resolve(superclass);

            EnterScope();
            Declare("super");
        }

        EnterScope();
        Declare("this");

        foreach (var method in classDecl.Methods)
            ResolveFunction(method.Parameters, method.Body);

        ExitScope();

        if (classDecl.Superclass is not null)
            ExitScope();

        return new();
    }

    private Unit ResolveWhileStmt(WhileStmt whileStmt)
    {
        Resolve(whileStmt.Condition);
        Resolve(whileStmt.Body);

        return new();
    }

    private Unit ResolveIfStmt(IfStmt ifStmt)
    {
        Resolve(ifStmt.Condition);
        Resolve(ifStmt.OnTrue);
        if (ifStmt.OnFalse.HasValue)
            Resolve(ifStmt.OnFalse.Value);

        return new();
    }

    private Unit ResolveFunDecl(FunDecl funDecl)
    {
        Declare(funDecl.Identifier.Name, new DeclaredName(funDecl.Identifier.Location));

        ResolveFunction(funDecl.Parameters, funDecl.Body);

        return new();
    }

    private void ResolveFunction(List<IdentifierInfo> parameters, List<Stmt> body)
    {
        EnterScope();

        foreach (var param in parameters)
            Declare(param.Name, new DeclaredName(param.Location));

        Resolve(body);
        ExitScope();
    }

    private Unit ResolveVarDecl(VarDecl varDecl)
    {
        Declare(varDecl.Identifier.Name, new DeclaredVariable(varDecl.Identifier.Location, false));
        if (varDecl.Initializer.HasValue)
            Resolve(varDecl.Initializer.Value);
        Define(varDecl.Identifier.Name);

        return new();
    }

    private void Define(string name)
    {
        if (scopes.Count is 0)
            return;

        var scope = scopes.Peek();
        if (scope[name] is DeclaredVariable variable)
            scope[name] = variable with { IsDefined = true };
    }

    private void Declare(string name, DeclaredName state)
    {
        if (scopes.Count is 0)
            return;

        if (!scopes.Peek().TryAdd(name, state))
            Runner.ReportError(state.Location, "Already a variable with this name in this scope.");
    }

    private void Declare(string name)
    {
        if (scopes.Count is 0)
            return;

        scopes.Peek()[name] = new ImplicitName();
    }

    private Unit ResolveBlock(Block block)
    {
        EnterScope();
        Resolve(block.Statements);
        ExitScope();

        return new();
    }

    private Unit Resolve(Expr expr) => expr switch
    {
        Variable varExpr => ResolveVarExpr(varExpr),
        AssignmentExpr assignmentExpr => ResolveAssignmentExpr(assignmentExpr),
        BinaryExpr binaryExpr => ResolveBinaryExpr(binaryExpr),
        CallExpr callExpr => ResolveCallExpr(callExpr),
        Grouping grouping => Resolve(grouping.Expr),
        UnaryExpr unary => Resolve(unary.Expr),
        LambdaExpr lambda => ResolveLambdaExpr(lambda),
        GetExpr getExpr => Resolve(getExpr.Instance),
        SetExpr setExpr => ResolveSetExpr(setExpr),
        Ternary ternary => ResolveTernary(ternary),
        ThisExpr thisExpr => ResolveThisExpr(thisExpr),
        SuperExpr superExpr => ResolveSuper(superExpr),
        IsExpr isExpr => ResolveIs(isExpr),
        SwitchExpr switchExpr => ResolveSwitch(switchExpr),

        Literal => new(),
    };

    private Unit ResolveSwitch(SwitchExpr switchExpr)
    {
        Resolve(switchExpr.Expr);

        foreach (var branch in switchExpr.Branches)
        {
            ResolvePattern(branch.PatternInfo.Pattern);
            Resolve(branch.Expr);
        }

        return new();
    }

    private Unit ResolveIs(IsExpr isExpr)
    {
        Resolve(isExpr.Expr);
        ResolvePattern(isExpr.PatternInfo.Pattern);

        return new();
    }

    private void ResolvePattern(Pattern pattern)
    {
        if (pattern is Variable variable)
            Resolve(variable);
    }

    private Unit ResolveSuper(SuperExpr superExpr)
    {
        ResolveLocal(superExpr, "super");
        return new();
    }

    private Unit ResolveThisExpr(ThisExpr thisExpr)
    {
        ResolveLocal(thisExpr, "this");
        return new();
    }

    private Unit ResolveTernary(Ternary ternary)
    {
        Resolve(ternary.Condition);
        Resolve(ternary.OnTrue);
        Resolve(ternary.OnFalse);

        return new();
    }

    private Unit ResolveSetExpr(SetExpr setExpr)
    {
        Resolve(setExpr.Value);
        Resolve(setExpr.Instance);

        return new();
    }

    private Unit ResolveLambdaExpr(LambdaExpr lambda)
    {
        ResolveFunction(lambda.Parameters, lambda.Body);
        return new();
    }

    private Unit ResolveCallExpr(CallExpr callExpr)
    {
        Resolve(callExpr.Callee);
        foreach (var arg in callExpr.Arguments)
            Resolve(arg);

        return new();
    }

    private Unit ResolveBinaryExpr(BinaryExpr binaryExpr)
    {
        Resolve(binaryExpr.Left);
        Resolve(binaryExpr.Right);

        return new();
    }

    private Unit ResolveAssignmentExpr(AssignmentExpr assignmentExpr)
    {
        Resolve(assignmentExpr.Value);
        ResolveLocal(assignmentExpr, assignmentExpr.Target.Name);

        return new();
    }

    private Unit ResolveVarExpr(Variable varExpr)
    {
        if (scopes.Count > 0 && scopes.Peek().TryGetValue(varExpr.Identifier.Name, out var state)
            && state is DeclaredVariable { IsDefined: false })
        {
            Runner.ReportError(varExpr.Identifier.Location, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(varExpr, varExpr.Identifier.Name);

        return new();
    }

    private void ResolveLocal(Expr expr, string name)
    {
        foreach (var (map, i) in scopes.Select((map, i) => (map, i)))
        {
            if (map.TryGetValue(name, out var state))
            {
                if (expr is not AssignmentExpr && state is DeclaredName declarable)
                    map[name] = declarable with { IsUsed = true };

                interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    private void EnterScope() => scopes.Push([]);

    private void ExitScope()
    {
        foreach (var state in scopes.Pop())
        {
            if (state.Value is DeclaredName { IsUsed: false } declarable)
                Runner.ReportWarn(declarable.Location, $"Unused declared name {state.Key}.");
        }
    }

    private readonly union NameState(DeclaredName, ImplicitName);

    private record DeclaredName(SourceLocation Location)
    {
        public bool IsUsed { get; init; }
    }

    private record DeclaredVariable(SourceLocation Location, bool IsDefined) : DeclaredName(Location);

    private record struct ImplicitName();
}
