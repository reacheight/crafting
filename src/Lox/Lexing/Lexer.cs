using System.Globalization;

namespace Lox.Lexing;

public class Lexer(string source)
{
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keywordMap = new()
    {
        ["and"] = new And(),
        ["class"] = new Class(),
        ["else"] = new Else(),
        ["false"] = new False(),
        ["fun"] = new Fun(),
        ["for"] = new For(),
        ["if"] = new If(),
        ["nil"] = new Nil(),
        ["or"] = new Or(),
        ["print"] = new Print(),
        ["return"] = new Return(),
        ["super"] = new Super(),
        ["this"] = new This(),
        ["true"] = new True(),
        ["var"] = new Var(),
        ["while"] = new While(),
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
                Runner.ReportError(line, error.Message);
        }

        yield return CreateToken(new(new Eof()));
    }

    private LexingUnit ScanLexingUnit() => Advance() switch
    {
        '(' => CreateToken(new LeftParen()),
        ')' => CreateToken(new RightParen()),
        '{' => CreateToken(new LeftBrace()),
        '}' => CreateToken(new RightBrace()),
        ',' => CreateToken(new Comma()),
        '.' => CreateToken(new Dot()),
        '-' => CreateToken(new Minus()),
        '+' => CreateToken(new Plus()),
        ';' => CreateToken(new Semicolon()),
        '*' => CreateToken(new Star()),
        ':' => CreateToken(new Colon()),
        '?' => CreateToken(new Question()),

        '!' => CreateToken(AdvanceIfMatch('=')
            ? new BangEqual()
            : new Bang()),
        '=' => CreateToken(AdvanceIfMatch('=')
            ? new EqualEqual()
            : new Equal()),
        '>' => CreateToken(AdvanceIfMatch('=')
            ? new GreaterEqual()
            : new Greater()),
        '<' => CreateToken(AdvanceIfMatch('=')
            ? new LessEqual()
            : new Less()),

        '/' when AdvanceIfMatch('/') => ScanComment(),
        '/' => CreateToken(new Slash()),

        ' ' or '\r' or '\t' => ScanWhitespace(),
        '\n' => ScanNewline(),

        '"' => ScanString(),

        var currentChar when char.IsDigit(currentChar) => ScanNumber(),

        var currentChar when char.IsLetter(currentChar) || currentChar == '_' => ScanIdentifierOrKeyword(),

        var currentChar => new SyntaxError($"Unexpected character: {currentChar}"),
    };

    private Token ScanIdentifierOrKeyword()
    {
        while (char.IsLetterOrDigit(Peek) || Peek == '_')
            Advance();

        var text = GetCurrentTokenText();
        return CreateToken(keywordMap.TryGetValue(text, out var keywordType)
            ? keywordType
            : new Identifier());
    }

    private Token ScanNumber()
    {
        ScanDigits();

        if (Peek == '.' && char.IsDigit(Next))
        {
            Advance();
            ScanDigits();
        }

        var val = double.Parse(GetCurrentTokenText(), CultureInfo.InvariantCulture);
        return CreateToken(new(new NumberLiterlToken(val)));

        void ScanDigits()
        {
            while (char.IsDigit(Peek))
                Advance();
        }
    }

    private LexingUnit ScanString()
    {
        while (!IsAtEnd && Peek != '"')
        {
            if (Peek == '\n')
                ScanNewline();

            Advance();
        }

        if (IsAtEnd)
            return new SyntaxError("Unterminated string.");

        Advance();

        var val = GetCurrentTokenText(1, -1);
        return CreateToken(new(new StringLiteralToken(val)));
    }

    private Token CreateToken(TokenType type)
        => new(GetCurrentTokenText(), line, type);

    private Comment ScanComment()
    {
        while (!IsAtEnd && Peek != '\n')
            Advance();

        return new();
    }

    private Whitespace ScanWhitespace()
    {
        while (Peek is ' ' or '\r' or '\t')
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

    private char Peek => source.ElementAtOrDefault(current);
    private char Next => source.ElementAtOrDefault(current + 1);

    private bool AdvanceIfMatch(char expected)
        => AdvanceIfMatch(c => c == expected);

    private bool AdvanceIfMatch(Predicate<char> predicate)
    {
        if (IsAtEnd || !predicate(Peek))
            return false;

        Advance();
        return true;
    }
}