using Lox.Lexing;

namespace Lox.Parsing;

public readonly union Stmt(ExprStmt, PrintStmt, VarStmt, Block, IfStmt);

public record ExprStmt(Expr Expr);
public record PrintStmt(Expr Expr);
public record VarStmt(Token Identifier, Expr? Initializer);
public record Block(List<Stmt> Statements);
public record IfStmt(Expr Condition, Stmt OnTrue, Stmt? OnFalse);
