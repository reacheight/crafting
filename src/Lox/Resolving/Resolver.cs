using Lox.Interpreting;
using Lox.Lexing;
using Lox.Parsing.Syntax;
using Lox.Utils;

namespace Lox.Resolving;

public class Resolver(Interpreter interpreter)
{
    private readonly Stack<Dictionary<string, VarState>> scopes = [];

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
        Declare(classDecl.Identifier, true);
        Define(classDecl.Identifier);

        if (classDecl.Superclass is { } superclass)
        {
            Resolve(superclass);

            EnterScope();
            scopes.Peek()["super"] = new(true, true, null);
        }

        EnterScope();
        scopes.Peek()["this"] = new(true, true, null);

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
        Declare(funDecl.Identifier);
        Define(funDecl.Identifier);

        ResolveFunction(funDecl.Parameters, funDecl.Body);

        return new();
    }

    private void ResolveFunction(List<IdentifierInfo> parameters, List<Stmt> body)
    {
        EnterScope();

        foreach (var param in parameters)
        {
            Declare(param, true);
            Define(param);
        }

        Resolve(body);
        ExitScope();
    }

    private Unit ResolveVarDecl(VarDecl varDecl)
    {
        Declare(varDecl.Identifier);
        if (varDecl.Initializer.HasValue)
            Resolve(varDecl.Initializer.Value);
        Define(varDecl.Identifier);

        return new();
    }

    private void Define(IdentifierInfo identifier)
    {
        if (scopes.Count is 0)
            return;

        var scope = scopes.Peek();
        scope[identifier.Name] = scope[identifier.Name] with { IsDefined = true };
    }

    private void Declare(IdentifierInfo identifier, bool isUsed = false)
    {
        if (scopes.Count is 0)
            return;

        if (!scopes.Peek().TryAdd(identifier.Name, new(false, isUsed, identifier.Location)))
            Runner.ReportError(identifier.Location, "Already a variable with this name in this scope.");
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

        Literal => new(),
    };

    private Unit ResolveSuper(SuperExpr superExpr)
    {
        ResolveLocal(superExpr, "super", false);
        return new();
    }

    private Unit ResolveThisExpr(ThisExpr thisExpr)
    {
        ResolveLocal(thisExpr, "this", false);
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
        ResolveLocal(assignmentExpr, assignmentExpr.Target.Name, false);

        return new();
    }

    private Unit ResolveVarExpr(Variable varExpr)
    {
        if (scopes.Count > 0 && scopes.Peek().TryGetValue(varExpr.Identifier.Name, out var state) && !state.IsDefined)
            Runner.ReportError(varExpr.Identifier.Location, "Can't read local variable in its own initializer.");

        ResolveLocal(varExpr, varExpr.Identifier.Name, true);

        return new();
    }

    private void ResolveLocal(Expr expr, string name, bool shouldUse)
    {
        foreach (var (map, i) in scopes.Select((map, i) => (map, i)))
        {
            if (map.TryGetValue(name, out var varState))
            {
                if (shouldUse)
                    map[name] = varState with { IsUsed = true };

                interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    private void EnterScope() => scopes.Push([]);

    private void ExitScope()
    {
        var scope = scopes.Pop();
        foreach (var unusedVar in scope.Where(pair => !pair.Value.IsUsed))
        {
            if (unusedVar.Value.Location is { } location)
                Runner.ReportError(location, $"Unused variable {unusedVar.Key}");
        }
    }

    // TODO: rewrite this hack with location nullability
    // "this" has no location since it's not declared explicitly and it's usage should not be tracked
    private record VarState(bool IsDefined, bool IsUsed, SourceLocation? Location);
}
