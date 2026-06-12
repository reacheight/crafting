using Lox.Lexing;

namespace Lox.Parsing;

public class Parser(List<Token> tokens)
{
    private int current = 0;

    public ParseResult Parse()
    {
        try
        {
            return Expression();
        }
        // TODO: get rid of throwing exception ?
        catch (ParseException parseException)
        {
            Runner.ReportError(parseException.Token, parseException.Message);
            return new ParseError(parseException.Token, parseException.Message);
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
