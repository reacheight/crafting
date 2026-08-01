using Lox.Lexing;

namespace Lox.Parsing.Syntax;

public readonly union Stmt(ExprStmt, PrintStmt, VarDecl, Block, IfStmt, WhileStmt, BreakStmt, FunDecl, ReturnStmt, ClassDecl);

public record ExprStmt(Expr Expr);
public record PrintStmt(Expr Expr);
public record VarDecl(IdentifierInfo Identifier, Expr? Initializer);
public record Block(List<Stmt> Statements);
public record IfStmt(Expr Condition, Stmt OnTrue, Stmt? OnFalse);
public record WhileStmt(Expr Condition, Stmt Body);
public record struct BreakStmt();
public record FunDecl(IdentifierInfo Identifier, List<IdentifierInfo> Parameters, List<Stmt> Body);
public record ReturnStmt(Expr? Expr, SourceLocation KeywordLocation);
public record ClassDecl(IdentifierInfo Identifier, List<FunDecl> Methods);
