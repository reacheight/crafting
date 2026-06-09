using Lox.Lexing;

namespace Lox.Parsing;

public class Parser(List<Token> tokens)
{
    private int current = 0;

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

        if (AdvanceIfMatch(new Question()))
        {
            var onTrue = Ternary();
            Consume(new Colon(), "Expect ':' in a ternary expression.");
            var onFalse = Ternary();
            expr = new Ternary(expr, onTrue, onFalse);
        }

        return expr;
    }

    private Expr Equality() => ParseBinaryExpr(Comparison, new EqualEqual(), new BangEqual());

    private Expr Comparison() => ParseBinaryExpr(Term, new Lexing.Less(), new Lexing.LessEqual(), new Lexing.Greater(), new Lexing.GreaterEqual());

    private Expr Term() => ParseBinaryExpr(Factor, new Plus(), new Minus());

    private Expr Factor() => ParseBinaryExpr(Unary, new Star(), new Slash());

    private Expr ParseBinaryExpr(Func<Expr> parseOperand, params TokenType[] operatorTypes)
    {
        var expr = parseOperand();

        while (AdvanceIfMatch(operatorTypes))
            expr = new BinaryExpr(expr, ToBinaryOperator(Previous.Type), parseOperand());

        return expr;
    }

    private Expr Unary() => AdvanceIfMatch(new Bang(), new Minus())
        ? new UnaryExpr(ToUnaryOperator(Previous.Type), Unary())
        : Primary();

    private Expr Primary()
    {
        if (AdvanceIfMatch(new True()))
            return new Literal(true);

        if (AdvanceIfMatch(new False()))
            return new Literal(false);

        if (AdvanceIfMatch(new Lexing.Nil()))
            return new Literal(new Nil());

        if (AdvanceIfLiteral() is var literal and not null)
        {
            return literal switch
            {
                StringLiteralToken strLiteral => new Literal(strLiteral.Value),
                NumberLiterlToken numLiteral => new Literal(numLiteral.Value),
            };
        }

        if (AdvanceIfMatch(new LeftParen()))
        {
            var expr = Expression();
            Consume(new RightParen(), "Expect ')' after expression.");
            return new Grouping(expr);
        }

        throw new ParseException(Peek, "Expect expression.");
    }

#pragma warning disable CS8509
    private static BinaryOperator ToBinaryOperator(TokenType type) => type switch
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
    };

    private static UnaryOperator ToUnaryOperator(TokenType type) => type switch
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
