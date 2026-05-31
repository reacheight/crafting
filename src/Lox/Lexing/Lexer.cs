using System.Globalization;

namespace Lox.Lexing;

public class Lexer(string source)
{
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, NonLiteralTokenType> keywordMap = new()
    {
        ["and"] = NonLiteralTokenType.And,
        ["class"] = NonLiteralTokenType.Class,
        ["else"] = NonLiteralTokenType.Else,
        ["false"] = NonLiteralTokenType.False,
        ["fun"] = NonLiteralTokenType.Fun,
        ["for"] = NonLiteralTokenType.For,
        ["if"] = NonLiteralTokenType.If,
        ["nil"] = NonLiteralTokenType.Nil,
        ["or"] = NonLiteralTokenType.Or,
        ["print"] = NonLiteralTokenType.Print,
        ["return"] = NonLiteralTokenType.Return,
        ["super"] = NonLiteralTokenType.Super,
        ["this"] = NonLiteralTokenType.This,
        ["true"] = NonLiteralTokenType.True,
        ["var"] = NonLiteralTokenType.Var,
        ["while"] = NonLiteralTokenType.While,
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

        yield return CreateToken(NonLiteralTokenType.Eof);
    }

    private LexingUnit ScanLexingUnit()
    {
        var currentChar = Advance();
        return currentChar switch
        {
            '(' => CreateToken(NonLiteralTokenType.LeftParen),
            ')' => CreateToken(NonLiteralTokenType.RightParen),
            '{' => CreateToken(NonLiteralTokenType.LeftBrace),
            '}' => CreateToken(NonLiteralTokenType.RightBrace),
            ',' => CreateToken(NonLiteralTokenType.Comma),
            '.' => CreateToken(NonLiteralTokenType.Dot),
            '-' => CreateToken(NonLiteralTokenType.Minus),
            '+' => CreateToken(NonLiteralTokenType.Plus),
            ';' => CreateToken(NonLiteralTokenType.Semicolon),
            '*' => CreateToken(NonLiteralTokenType.Star),

            '!' => CreateToken(AdvanceIfMatch('=')
                    ? NonLiteralTokenType.BangEqual
                    : NonLiteralTokenType.Bang),
            '=' => CreateToken(AdvanceIfMatch('=')
                    ? NonLiteralTokenType.EqualEqual
                    : NonLiteralTokenType.Equal),
            '>' => CreateToken(AdvanceIfMatch('=')
                    ? NonLiteralTokenType.GreaterEqual
                    : NonLiteralTokenType.Greater),
            '<' => CreateToken(AdvanceIfMatch('=')
                    ? NonLiteralTokenType.LessEqual
                    : NonLiteralTokenType.Less),

            '/' when AdvanceIfMatch('/') => ScanComment(),
            '/' => CreateToken(NonLiteralTokenType.Slash),

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
                : NonLiteralTokenType.Identifier);
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
        return CreateToken(LiteralTokenType.Number, new(val));

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
        return CreateToken(LiteralTokenType.String, new(val));
    }

    private Token CreateToken(NonLiteralTokenType type)
        => new(GetCurrentTokenText(), line, new(type));

    private Token CreateToken(LiteralTokenType type, TokenLiteral literal)
        => new(GetCurrentTokenText(), line, new(new LiteralToken(type, literal)));

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