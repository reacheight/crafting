using System.Globalization;

namespace Lox.Lexing;

public class Lexer(string source)
{
    private readonly List<Token> tokens = [];

    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keywordMap = new()
    {
        ["and"] = TokenType.AND,
        ["class"] = TokenType.CLASS,
        ["else"] = TokenType.ELSE,
        ["false"] = TokenType.FALSE,
        ["fun"] = TokenType.FUN,
        ["for"] = TokenType.FOR,
        ["if"] = TokenType.IF,
        ["nil"] = TokenType.NIL,
        ["or"] = TokenType.OR,
        ["print"] = TokenType.PRINT,
        ["return"] = TokenType.RETURN,
        ["super"] = TokenType.SUPER,
        ["this"] = TokenType.THIS,
        ["true"] = TokenType.TRUE,
        ["var"] = TokenType.VAR,
        ["while"] = TokenType.WHILE,
    };

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
                : TokenType.IDENTIFIER);
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

        var val = GetCurrentTokenText(1, -1);
        return CreateToken(TokenType.STRING, new(val));
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