namespace Lox.Parsing;

public readonly union Stmt(ExprStmt, PrintStmt);

public record ExprStmt(Expr Expr);
public record PrintStmt(Expr Expr);
