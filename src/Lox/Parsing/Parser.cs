using Lox.Lexing;

namespace Lox.Parsing;

public class Parser(List<Token> tokens)
{
    private int current = 0;

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
            Runner.ReportError(parseException.Token, parseException.Message);
            return new ParseError(parseException.Token, parseException.Message);
        }
    }

    private Stmt Declaration()
    {
        if (AdvanceIfMatch(new Var()))
            return VarDeclaration();

        return Statement();
    }

    private Stmt VarDeclaration()
    {
        var identifier = Consume(new Identifier(), "Expect variable name after 'var'.");
        var initializer = AdvanceIfMatch(new Lexing.Equal())
            ? Expression()
            : (Expr?)null;

        Consume(new Semicolon(), "Expect ';' after variable declaration.");

        return new VarStmt(identifier, initializer);
    }

    private Stmt Statement()
    {
        if (AdvanceIfMatch(new Print()))
            return PrintStatement();

        if (AdvanceIfMatch(new LeftBrace()))
            return new Block(Block());

        if (AdvanceIfMatch(new If()))
            return IfStatement();

        return ExpressionStatement();
    }

    private Stmt IfStatement()
    {
        Consume(new LeftParen(), "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(new RightParen(), "Expect ')' after if condition.");

        var onTrue = Statement();
        var onFalse = AdvanceIfMatch(new Else())
            ? Statement()
            : (Stmt?)null;

        return new IfStmt(condition, onTrue, onFalse);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();
        while (Peek.Type is not (RightBrace or Eof))
            statements.Add(Declaration());
        Consume(new RightBrace(), "Expect '}' after block.");
        return statements;
    }

    private Stmt PrintStatement()
    {
        var expr = Expression();
        Consume(new Semicolon(), "Expect ';' after value.");
        return new PrintStmt(expr);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(new Semicolon(), "Expect ';' after expression.");
        return new ExprStmt(expr);
    }

    private Expr Expression() => Assignment();

    private Expr Assignment()
    {
        var expr = Ternary();

        if (AdvanceIfMatch(new Lexing.Equal()))
        {
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

        if (AdvanceIfMatch(new Question()))
        {
            var onTrue = Ternary();
            Consume(new Colon(), "Expect ':' in a ternary expression.");
            var onFalse = Ternary();
            expr = new Ternary(expr, onTrue, onFalse);
        }

        return expr;
    }

    private Expr OrExpr() => ParseBinaryExpr(AndExpr, new Or());

    private Expr AndExpr() => ParseBinaryExpr(Equality, new And());

    private Expr Equality() => ParseBinaryExpr(Comparison, new EqualEqual(), new BangEqual());

    private Expr Comparison() => ParseBinaryExpr(Term, new Lexing.Less(), new Lexing.LessEqual(), new Lexing.Greater(), new Lexing.GreaterEqual());

    private Expr Term() => ParseBinaryExpr(Factor, new Plus(), new Minus());

    private Expr Factor() => ParseBinaryExpr(Unary, new Star(), new Slash());

    private Expr ParseBinaryExpr(Func<Expr> parseOperand, params TokenType[] operatorTypes)
    {
        var expr = parseOperand();

        while (AdvanceIfMatch(operatorTypes))
            expr = new BinaryExpr(expr, new BinaryOperator(ToBinaryOperatorType(Previous.Type), Previous), parseOperand());

        return expr;
    }

    private Expr Unary() => AdvanceIfMatch(new Bang(), new Minus())
        ? new UnaryExpr(new(ToUnaryOperator(Previous.Type), Previous), Unary())
        : Primary();

    private Expr Primary()
    {
        if (AdvanceIfMatch(new True()))
            return new Literal(true, Previous);

        if (AdvanceIfMatch(new False()))
            return new Literal(false, Previous);

        if (AdvanceIfMatch(new Lexing.Nil()))
            return new Literal(new Nil(), Previous);

        if (AdvanceIfLiteral() is var literal and not null)
        {
            return literal switch
            {
                StringLiteralToken strLiteral => new Literal(strLiteral.Value, Previous),
                NumberLiterlToken numLiteral => new Literal(numLiteral.Value, Previous),
            };
        }

        if (AdvanceIfMatch(new LeftParen()))
        {
            var expr = Expression();
            Consume(new RightParen(), "Expect ')' after expression.");
            return new Grouping(expr);
        }

        if (AdvanceIfMatch(new Identifier()))
            return new Variable(Previous);

        throw new ParseException(Peek, "Expect expression.");
    }

#pragma warning disable CS8509
    private static BinaryOperatorType ToBinaryOperatorType(TokenType type) => type switch
    {
        EqualEqual => new Equal(),
        BangEqual => new NotEqual(),
        Lexing.Less => new Less(),
        Lexing.LessEqual => new LessEqual(),
        Lexing.Greater => new Greater(),
        Lexing.GreaterEqual => new GreaterEqual(),
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

    private Token Consume(TokenType type, string errorMessage)
    {
        if (IsAt(type))
            return Advance();

        throw new ParseException(Peek, errorMessage);
    }

    private bool AdvanceIfMatch(params IEnumerable<TokenType> types)
    {
        if (!types.Any(IsAt))
            return false;

        Advance();
        return true;
    }

    private LiteralToken? AdvanceIfLiteral()
    {
        if (Peek.Type is not LiteralToken peekLiteral)
            return null;

        Advance();
        return peekLiteral;
    }

    private Token Advance()
    {
        if (!IsAtEnd)
            current++;
        return Previous;
    }

    private bool IsAtEnd => IsAt(new Eof());

    private bool IsAt(TokenType type)
        => Peek.Type.Value.Equals(type.Value);

    private Token Peek => tokens[current];
    private Token Previous => tokens[current - 1];
}
