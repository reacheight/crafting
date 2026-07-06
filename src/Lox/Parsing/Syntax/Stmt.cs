namespace Lox.Parsing.Syntax;

public readonly union Stmt(ExprStmt, PrintStmt, VarStmt, Block, IfStmt, WhileStmt, BreakStmt, FunStmt);

public record ExprStmt(Expr Expr);
public record PrintStmt(Expr Expr);
public record VarStmt(IdentifierInfo Identifier, Expr? Initializer);
public record Block(List<Stmt> Statements);
public record IfStmt(Expr Condition, Stmt OnTrue, Stmt? OnFalse);
public record WhileStmt(Expr Condition, Stmt Body);
public record struct BreakStmt();
public record FunStmt(IdentifierInfo Identifier, List<IdentifierInfo> Parameters, List<Stmt> Body);
