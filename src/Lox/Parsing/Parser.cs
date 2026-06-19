using Lox.Lexing;
using Lox.Parsing.Syntax;

namespace Lox.Parsing;

public class Parser(List<Token> tokens)
{
    private int current = 0;
    private int loopsCount = 0;

    public ParseResult Parse()
    {
        try
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd)
                statements.Add(Declaration());

            return new LoxProgram(statements);
        }
        // TODO: get rid of throwing exception ?
        catch (ParseException parseException)
        {
            Runner.ReportError(parseException.Token.Location, parseException.Message);
            return new ParseError(parseException.Token, parseException.Message);
        }
    }

    private Stmt Declaration() => Peek.Type switch
    {
        Var => AdvanceAnd(VarDeclaration),
        _ => Statement(),
    };

    private Stmt VarDeclaration()
    {
        var identifierInfo = Peek.Type is Identifier ident
            ? AdvanceAnd(() => new IdentifierInfo(ident.Name, Previous.Location))
            : throw new ParseException(Peek, "Expect variable name after 'var'.");

        var initializer = Peek.Type is Lexing.Equal
            ? AdvanceAnd(Expression)
            : (Expr?)null;

        Consume<Semicolon>("Expect ';' after variable declaration.");

        return new VarStmt(identifierInfo, initializer);
    }

    private Stmt Statement() => Peek.Type switch
    {
        Print => AdvanceAnd(PrintStatement),
        LeftBrace => AdvanceAnd(() => new Block(Block())),
        If => AdvanceAnd(IfStatement),
        While => AdvanceAnd(WhileStatement),
        For => AdvanceAnd(ForLoop),
        Break => AdvanceAnd(BreakStatement),
        _ => ExpressionStatement(),
    };

    private Stmt BreakStatement()
    {
        if (loopsCount is 0)
            throw new ParseException(Previous, "'break' can't be used outside while and for loops.");

        Consume<Semicolon>("Expect ';' after 'break'.");

        return new BreakStmt();
    }

    private Stmt ForLoop()
    {
        loopsCount++;

        Consume<LeftParen>("Expect '(' after 'for'.");

        var initializer = Peek.Type is Semicolon
            ? AdvanceAnd(() => (Stmt?)null)
            : Peek.Type is Var
                ? AdvanceAnd(VarDeclaration)
                : ExpressionStatement();

        var condition = Peek.Type is Semicolon
            ? (Expr?)null
            : Expression();
        Consume<Semicolon>("Expect ';' after loop condition.");

        var increment = Peek.Type is RightParen
            ? (Expr?)null
            : Expression();
        Consume<RightParen>("Expect ')' after for clauses.");

        var body = Statement();
        if (increment.HasValue)
            body = new Block([body, new ExprStmt(increment.Value)]);

        var whileCondition = condition ?? new Literal(true);
        body = new WhileStmt(whileCondition, body);

        if (initializer.HasValue)
            body = new Block([initializer.Value, body]);

        loopsCount--;

        return body;
    }

    private Stmt WhileStatement()
    {
        loopsCount++;

        Consume<LeftParen>("Expect '(' after 'while'.");
        var condition = Expression();
        Consume<RightParen>("Expect ')' after while condition.");
        var body = Statement();

        loopsCount--;

        return new WhileStmt(condition, body);
    }

    private Stmt IfStatement()
    {
        Consume<LeftParen>("Expect '(' after 'if'.");
        var condition = Expression();
        Consume<RightParen>("Expect ')' after if condition.");

        var onTrue = Statement();
        var onFalse = Peek.Type is Else
            ? AdvanceAnd(Statement)
            : (Stmt?)null;

        return new IfStmt(condition, onTrue, onFalse);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();
        while (Peek.Type is not (RightBrace or Eof))
            statements.Add(Declaration());
        Consume<RightBrace>("Expect '}' after block.");
        return statements;
    }

    private Stmt PrintStatement()
    {
        var expr = Expression();
        Consume<Semicolon>("Expect ';' after value.");
        return new PrintStmt(expr);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume<Semicolon>("Expect ';' after expression.");
        return new ExprStmt(expr);
    }

    private Expr Expression() => Assignment();

    private Expr Assignment()
    {
        var expr = Ternary();

        if (Peek.Type is Lexing.Equal)
        {
            Advance();

            var equal = Previous;
            var val = Assignment();

            if (expr.Value is Variable target)
                return new AssignmentExpr(target.Identifier, val);

            throw new ParseException(equal, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Ternary()
    {
        var expr = OrExpr();

        if (Peek.Type is Question)
        {
            Advance();

            var onTrue = Ternary();
            Consume<Colon>("Expect ':' in a ternary expression.");
            var onFalse = Ternary();
            expr = new Ternary(expr, onTrue, onFalse);
        }

        return expr;
    }

    private Expr OrExpr() => ParseBinaryExpr(AndExpr, _ => _ is Or);

    private Expr AndExpr() => ParseBinaryExpr(Equality, _ => _ is And);

    private Expr Equality() => ParseBinaryExpr(Comparison, _ => _ is EqualEqual or BangEqual);

    private Expr Comparison() => ParseBinaryExpr(Term, _ => _ is Lexing.Less or Lexing.LessEqual or Lexing.Greater or Lexing.GreaterEqual);

    private Expr Term() => ParseBinaryExpr(Factor, _ => _ is Plus or Minus);

    private Expr Factor() => ParseBinaryExpr(Unary, _ => _ is Star or Slash);

    private Expr ParseBinaryExpr(Func<Expr> parseOperand, Predicate<TokenType> predicate)
    {
        var expr = parseOperand();

        while (AdvanceIfMatch(predicate))
            expr = new BinaryExpr(expr, new BinaryOperator(ToBinaryOperatorType(Previous.Type), Previous.Location), parseOperand());

        return expr;
    }

    private Expr Unary() => AdvanceIfMatch(_ => _ is Bang or Minus)
        ? new UnaryExpr(new(ToUnaryOperator(Previous.Type), Previous.Location), Unary())
        : Primary();

    private Expr Primary() => Peek.Type switch
    {
        True => AdvanceAnd(() => new Literal(true)),
        False => AdvanceAnd(() => new Literal(false)),
        Lexing.Nil => AdvanceAnd(() => new Literal(new Nil())),
        Identifier ident => AdvanceAnd(() => new Variable(new(ident.Name, Peek.Location))),
        LiteralToken literal => AdvanceAnd(() => literal switch
        {
            StringLiteralToken strLiteral => new Literal(strLiteral.Value),
            NumberLiterlToken numLiteral => new Literal(numLiteral.Value),
        }),
        LeftParen => AdvanceAnd(() =>
        {
            var expr = Expression();
            Consume<RightParen>("Expect ')' after expression.");
            return new Grouping(expr);
        }),
        _ => throw new ParseException(Peek, "Expect expression."),
    };

#pragma warning disable CS8509
    private static BinaryOperatorType ToBinaryOperatorType(TokenType type) => type switch
    {
        EqualEqual => new Syntax.Equal(),
        BangEqual => new NotEqual(),
        Lexing.Less => new Syntax.Less(),
        Lexing.LessEqual => new Syntax.LessEqual(),
        Lexing.Greater => new Syntax.Greater(),
        Lexing.GreaterEqual => new Syntax.GreaterEqual(),
        Plus => new Add(),
        Minus => new Substract(),
        Slash => new Divide(),
        Star => new Multiply(),
        And => new LogicalAnd(),
        Or => new LogicalOr(),
    };

    private static UnaryOperatorType ToUnaryOperator(TokenType type) => type switch
    {
        Minus => new Negate(),
        Bang => new Not(),
    };
#pragma warning restore CS8509

    private void Consume<T>(string errorMessage)
    {
        if (Peek.Type is not T)
            throw new ParseException(Peek, errorMessage);

        Advance();
    }

    private bool AdvanceIfMatch(Predicate<TokenType> predicate)
    {
        if (!predicate(Peek.Type))
            return false;

        Advance();
        return true;
    }

    private T AdvanceAnd<T>(Func<T> parse)
    {
        Advance();
        return parse();
    }

    private Token Advance()
    {
        if (!IsAtEnd)
            current++;
        return Previous;
    }

    private bool IsAtEnd => Peek.Type is Eof;

    private Token Peek => tokens[current];
    private Token Previous => tokens[current - 1];
}
