using System.Globalization;

namespace Lox.Lexing;

public class Lexer(string source)
{
    private readonly List<Token> tokens = [];

    private int start = 0;
    private int current = 0;
    private int line = 1;

    private bool IsAtEnd => current >= source.Length;

    public IEnumerable<Token> Tokenize()
    {
        while (!IsAtEnd)
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new(TokenType.EOF, "", null, line));
        return tokens;
    }


    private void ScanToken()
    {
        var lexingUnit = ScanLexingUnit();
        if (lexingUnit is Token token)
            tokens.Add(token);
        else if (lexingUnit is SyntaxError error)
            Runner.Error(line, $"[syntax error] {error.Message}");
    }

    private LexingUnit ScanLexingUnit()
    {
        var currentChar = Advance();
        return currentChar switch
        {
            '(' => CreateToken(TokenType.LEFT_PAREN),
            ')' => CreateToken(TokenType.RIGHT_PAREN),
            '{' => CreateToken(TokenType.LEFT_BRACE),
            '}' => CreateToken(TokenType.RIGHT_BRACE),
            ',' => CreateToken(TokenType.COMMA),
            '.' => CreateToken(TokenType.DOT),
            '-' => CreateToken(TokenType.MINUS),
            '+' => CreateToken(TokenType.PLUS),
            ';' => CreateToken(TokenType.SEMICOLON),
            '*' => CreateToken(TokenType.STAR),

            '!' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.BANG_EQUAL
                    : TokenType.BANG),
            '=' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.EQUAL_EQUAL
                    : TokenType.EQUAL),
            '>' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.GREATER_EQUAL
                    : TokenType.GREATER),
            '<' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.LESS_EQUAL
                    : TokenType.LESS),

            '/' when AdvanceIfMatch('/') => ScanComment(),
            '/' => CreateToken(TokenType.SLASH),

            ' ' or '\r' or '\t' => ScanWhitespace(),
            '\n' => ScanNewline(),

            '"' => ScanString(),
            _ when char.IsDigit(currentChar) => ScanNumber(),

            _ => new SyntaxError($"Unexpected character: {currentChar}"),
        };
    }

    private Token ScanNumber()
    {
        ScanDigits();

        if (Peek() == '.' && char.IsDigit(Peek(1)))
        {
            Advance();
            ScanDigits();
        }

        var val = double.Parse(source[start..current], CultureInfo.InvariantCulture);
        return CreateToken(TokenType.NUMBER, new(val));

        void ScanDigits()
        {
            while (char.IsDigit(Peek()))
                Advance();
        }
    }

    private LexingUnit ScanString()
    {
        while (!IsAtEnd && Peek() != '"')
        {
            if (Peek() == '\n')
                ScanNewline();

            Advance();
        }

        if (IsAtEnd)
            return new SyntaxError("Unterminated string.");

        Advance();

        var val = source[(start + 1)..(current - 1)];
        return CreateToken(TokenType.STRING, new(val));
    }

    private Token CreateToken(TokenType type, TokenLiteral? literal = null)
    {
        var text = source[start..current];
        return new(type, text, literal, line);
    }

    private Comment ScanComment()
    {
        while (!IsAtEnd && Peek() != '\n')
            Advance();

        return new();
    }

    private Whitespace ScanWhitespace()
    {
        while (Peek() is ' ' or '\r' or '\t')
            Advance();

        return new();
    }

    private Whitespace ScanNewline()
    {
        line++;
        return new();
    }

    private char Advance() => source[current++];
    private char Peek(int offset = 0) => source.ElementAtOrDefault(current + offset);

    private bool AdvanceIfMatch(char expected)
        => AdvanceIfMatch(c => c == expected);

    private bool AdvanceIfMatch(Predicate<char> predicate)
    {
        if (IsAtEnd || !predicate(Peek()))
            return false;

        current++;
        return true;
    }
}