using Lox.Lexing;

namespace Lox.Parsing;

public readonly union Stmt(ExprStmt, PrintStmt, VarStmt);

public record ExprStmt(Expr Expr);
public record PrintStmt(Expr Expr);
public record VarStmt(Token Identifier, Expr? Initializer = null);
