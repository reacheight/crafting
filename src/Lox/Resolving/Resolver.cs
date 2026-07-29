using Lox.Interpreting;
using Lox.Lexing;
using Lox.Parsing.Syntax;
using Lox.Utils;

namespace Lox.Resolving;

public class Resolver(Interpreter interpreter)
{
    private record VarState(bool IsDefined, bool IsUsed, SourceLocation Location);
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

        _ => new(),
    };

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

        _ => new(),
    };

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
        ResolveLocal(assignmentExpr, assignmentExpr.Target, false);

        return new();
    }

    private Unit ResolveVarExpr(Variable varExpr)
    {
        if (scopes.Count > 0 && scopes.Peek().TryGetValue(varExpr.Identifier.Name, out var state) && !state.IsDefined)
            Runner.ReportError(varExpr.Identifier.Location, "Can't read local variable in its own initializer.");

        ResolveLocal(varExpr, varExpr.Identifier, true);

        return new();
    }

    private void ResolveLocal(Expr expr, IdentifierInfo identifier, bool shouldUse)
    {
        foreach (var (map, i) in scopes.Select((map, i) => (map, i)))
        {
            if (map.TryGetValue(identifier.Name, out var varState))
            {
                if (shouldUse)
                    map[identifier.Name] = varState with { IsUsed = true };

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
            Runner.ReportError(unusedVar.Value.Location, $"Unused variable {unusedVar.Key}");
    }
}
