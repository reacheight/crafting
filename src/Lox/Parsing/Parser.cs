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
        catch (ParseException parseException)
        {
            Runner.ReportError(parseException.Token, parseException.Message);
            return null;
        }
    }

    private Expr Expression() => Ternary();

    private Expr Ternary()
    {
        var expr = Equality();

        if (AdvanceIfMatch(NonLiteralTokenType.Question))
        {
            var onTrue = Ternary();
            Consume(NonLiteralTokenType.Colon, "Expect ':' in a ternary expression.");
            var onFalse = Ternary();
            expr = new Ternary(expr, onTrue, onFalse);
        }

        return expr;
    }

    private Expr Equality() => ParseBinaryExpr(Comparison, NonLiteralTokenType.EqualEqual, NonLiteralTokenType.BangEqual);

    private Expr Comparison() => ParseBinaryExpr(Term, NonLiteralTokenType.Less, NonLiteralTokenType.LessEqual, NonLiteralTokenType.Greater, NonLiteralTokenType.GreaterEqual);

    private Expr Term() => ParseBinaryExpr(Factor, NonLiteralTokenType.Plus, NonLiteralTokenType.Minus);

    private Expr Factor() => ParseBinaryExpr(Unary, NonLiteralTokenType.Star, NonLiteralTokenType.Slash);

    private Expr ParseBinaryExpr(Func<Expr> parseOperand, params NonLiteralTokenType[] operatorTypes)
    {
        var expr = parseOperand();

        while (AdvanceIfMatch(operatorTypes))
            expr = new BinaryExpr(expr, tokenToBinaryOperator[Previous.Type], parseOperand());

        return expr;
    }

    private Expr Unary() => AdvanceIfMatch(NonLiteralTokenType.Bang, NonLiteralTokenType.Minus)
            ? new UnaryExpr(tokenToUnaryOperator[Previous.Type], Unary())
            : Primary();

    private Expr Primary()
    {
        if (AdvanceIfMatch(NonLiteralTokenType.True))
            return new Literal(true);

        if (AdvanceIfMatch(NonLiteralTokenType.False))
            return new Literal(false);

        if (AdvanceIfMatch(NonLiteralTokenType.Nil))
            return new Literal(new Nil());

        if (AdvanceIfLiteral() is LiteralToken literal)
        {
            return literal.Literal switch
            {
                string s => new Literal(s),
                double n => new Literal(n),
            };
        }

        if (AdvanceIfMatch(NonLiteralTokenType.LeftParen))
        {
            var expr = Expression();
            Consume(NonLiteralTokenType.RightParen, "Expect ')' after expression.");
            return new Grouping(expr);
        }

        throw new ParseException(Peek, "Expect expression.");
    }

    private Token Consume(NonLiteralTokenType type, string errorMessage)
    {
        if (IsAt(type))
            return Advance();

        throw new ParseException(Peek, errorMessage);
    }

    private bool AdvanceIfMatch(params IEnumerable<NonLiteralTokenType> types)
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

    private bool IsAtEnd => IsAt(NonLiteralTokenType.Eof);

    private bool IsAt(NonLiteralTokenType type)
        => Peek.Type is NonLiteralTokenType peekType && type == peekType;

    private Token Peek => tokens[current];
    private Token Previous => tokens[current - 1];
}
