using System.Globalization;

namespace Lox.Lexing;

public class Lexer(string source)
{
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keywordMap = new()
    {
        ["and"] = TokenType.And,
        ["class"] = TokenType.Class,
        ["else"] = TokenType.Else,
        ["false"] = TokenType.False,
        ["fun"] = TokenType.Fun,
        ["for"] = TokenType.For,
        ["if"] = TokenType.If,
        ["nil"] = TokenType.Nil,
        ["or"] = TokenType.Or,
        ["print"] = TokenType.Print,
        ["return"] = TokenType.Return,
        ["super"] = TokenType.Super,
        ["this"] = TokenType.This,
        ["true"] = TokenType.True,
        ["var"] = TokenType.Var,
        ["while"] = TokenType.While,
    };

    public IEnumerable<Token> Tokenize()
    {
        while (!IsAtEnd)
        {
            start = current;

            var lexingUnit = ScanLexingUnit();
            if (lexingUnit is Token token)
                yield return token;
            else if (lexingUnit is SyntaxError error)
                Runner.Error(line, $"[syntax error] {error.Message}");
        }

        yield return new(TokenType.Eof, "", null, line);
    }

    private LexingUnit ScanLexingUnit()
    {
        var currentChar = Advance();
        return currentChar switch
        {
            '(' => CreateToken(TokenType.LeftParen),
            ')' => CreateToken(TokenType.RightParen),
            '{' => CreateToken(TokenType.LeftBrace),
            '}' => CreateToken(TokenType.RightBrace),
            ',' => CreateToken(TokenType.Comma),
            '.' => CreateToken(TokenType.Dot),
            '-' => CreateToken(TokenType.Minus),
            '+' => CreateToken(TokenType.Plus),
            ';' => CreateToken(TokenType.Semicolon),
            '*' => CreateToken(TokenType.Star),

            '!' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.BangEqual
                    : TokenType.Bang),
            '=' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.EqualEqual
                    : TokenType.Equal),
            '>' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.GreaterEqual
                    : TokenType.Greater),
            '<' => CreateToken(AdvanceIfMatch('=')
                    ? TokenType.LessEqual
                    : TokenType.Less),

            '/' when AdvanceIfMatch('/') => ScanComment(),
            '/' => CreateToken(TokenType.Slash),

            ' ' or '\r' or '\t' => ScanWhitespace(),
            '\n' => ScanNewline(),

            '"' => ScanString(),

            _ when char.IsDigit(currentChar) => ScanNumber(),

            _ when char.IsLetter(currentChar) || currentChar == '_' => ScanIdentifierOrKeyword(),

            _ => new SyntaxError($"Unexpected character: {currentChar}"),
        };
    }

    private Token ScanIdentifierOrKeyword()
    {
        while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
            Advance();

        var text = GetCurrentTokenText();
        return CreateToken(keywordMap.TryGetValue(text, out var keywordType)
                ? keywordType
                : TokenType.Identifier);
    }

    private Token ScanNumber()
    {
        ScanDigits();

        if (Peek() == '.' && char.IsDigit(Peek(1)))
        {
            Advance();
            ScanDigits();
        }

        var val = double.Parse(GetCurrentTokenText(), CultureInfo.InvariantCulture);
        return CreateToken(TokenType.Number, new(val));

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

        var val = GetCurrentTokenText(1, -1);
        return CreateToken(TokenType.String, new(val));
    }

    private Token CreateToken(TokenType type, TokenLiteral? literal = null)
        => new(type, GetCurrentTokenText(), literal, line);

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

    private bool IsAtEnd => current >= source.Length;

    private string GetCurrentTokenText(int startOffset = 0, int currentOffset = 0)
        => source[(start + startOffset)..(current + currentOffset)];

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