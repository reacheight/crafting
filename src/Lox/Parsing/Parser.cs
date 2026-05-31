namespace Lox.Parsing;

public class Parser(List<Token> tokens)
{
    private int current = 0;

    private static readonly Dictionary<TokenType, BinaryOperator> tokenToBinaryOperator = new()
    {
        [NonLiteralTokenType.BangEqual] = new NotEqual(),
        [NonLiteralTokenType.EqualEqual] = new Equal(),
        [NonLiteralTokenType.Less] = new Less(),
        [NonLiteralTokenType.LessEqual] = new LessEqual(),
        [NonLiteralTokenType.Greater] = new Greater(),
        [NonLiteralTokenType.GreaterEqual] = new GreaterEqual(),
        [NonLiteralTokenType.Plus] = new Add(),
        [NonLiteralTokenType.Minus] = new Substract(),
        [NonLiteralTokenType.Slash] = new Divide(),
        [NonLiteralTokenType.Star] = new Multiply(),
    };

    private static readonly Dictionary<TokenType, UnaryOperator> tokenToUnaryOperator = new()
    {
        [NonLiteralTokenType.Minus] = new Negate(),
        [NonLiteralTokenType.Bang] = new Not(),
    };

    public Expr? Parse()
    {
        try
        {
            return Expression();
        }
        catch
        {
            return null;
        }
    }

    private Expr Expression() => Equality();

    private Expr Equality() => ParseBinaryExpr(Comparison, NonLiteralTokenType.EqualEqual, NonLiteralTokenType.BangEqual);

    private Expr Comparison() => ParseBinaryExpr(Term, NonLiteralTokenType.Less, NonLiteralTokenType.LessEqual, NonLiteralTokenType.Greater, NonLiteralTokenType.GreaterEqual);

    private Expr Term() => ParseBinaryExpr(Factor, NonLiteralTokenType.Plus, NonLiteralTokenType.Minus);

    private Expr Factor() => ParseBinaryExpr(Unary, NonLiteralTokenType.Star, NonLiteralTokenType.Slash);

    private Expr ParseBinaryExpr(Func<Expr> parseOperand, params NonLiteralTokenType[] operatorTypes)
    {
        var expr = parseOperand();

        while (MatchNonLiteral(operatorTypes))
            expr = new BinaryExpr(expr, tokenToBinaryOperator[Previous.Type], parseOperand());

        return expr;
    }

    private Expr Unary() => MatchNonLiteral(NonLiteralTokenType.Bang, NonLiteralTokenType.Minus)
            ? new UnaryExpr(tokenToUnaryOperator[Previous.Type], Unary())
            : Primary();

    private Expr Primary()
    {
        if (MatchNonLiteral(NonLiteralTokenType.True))
            return new Literal(true);

        if (MatchNonLiteral(NonLiteralTokenType.False))
            return new Literal(false);

        if (MatchNonLiteral(NonLiteralTokenType.Nil))
            return new Literal(new Nil());

        if (Peek.Type is LiteralToken peekLiteralToken)
        {
            Advance();
            return peekLiteralToken.Literal switch
            {
                string s => new Literal(s),
                double n => new Literal(n),
            };
        }

        if (MatchNonLiteral(NonLiteralTokenType.LeftParen))
        {
            var expr = Expression();
            Consume(NonLiteralTokenType.RightParen, "Expect ')' after expression.");
            return new Grouping(expr);
        }

        throw ReportAndCreateException(Peek, "Expect expression.");
    }

    private Token Consume(NonLiteralTokenType type, string errorMessage)
    {
        if (IsAtNonLiteralToken(type))
            return Advance();

        throw ReportAndCreateException(Peek, errorMessage);
    }

    private static ParseException ReportAndCreateException(Token token, string message)
    {
        Runner.ReportError(token, message);
        return new ParseException(message);
    }

    private bool MatchNonLiteral(params IEnumerable<NonLiteralTokenType> types)
    {
        foreach (var type in types)
        {
            if (IsAtNonLiteralToken(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Advance()
    {
        if (!IsAtEnd)
            current++;
        return Previous;
    }

    private bool IsAtNonLiteralToken(NonLiteralTokenType type)
        => Peek.Type is NonLiteralTokenType peekType && type == peekType;

    private bool IsAtEnd => IsAtNonLiteralToken(NonLiteralTokenType.Eof);
    private Token Peek => tokens[current];
    private Token Previous => tokens[current - 1];
}
