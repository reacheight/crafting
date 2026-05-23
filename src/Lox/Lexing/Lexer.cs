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

            '/' when AdvanceIfMatch('/') => HandleComment(),
            '/' => CreateToken(TokenType.SLASH),

            ' ' or '\r' or '\t' => HandleWhitespace(),
            '\n' => HandleLinebreak(),

            '"' => HandleString(),

            _ => new SyntaxError($"Unexpected character: {currentChar}"),
        };
    }

    private LexingUnit HandleString()
    {
        while (!IsAtEnd && Peek() != '"')
        {
            if (Peek() == '\n')
                line++;

            Advance();
        }

        if (IsAtEnd)
            return new SyntaxError("Unterminated string.");

        Advance();

        var val = source[(start + 1)..(current - 1)];
        return CreateToken(TokenType.STRING, new(val));
    }

    private char Advance() => source[current++];
    private char Peek() => source[current];

    private bool AdvanceIfMatch(char expected)
    {
        if (IsAtEnd || Peek() != expected)
            return false;

        current++;
        return true;
    }

    private Token CreateToken(TokenType type, TokenLiteral? literal = null)
    {
        var text = source[start..current];
        return new(type, text, literal, line);
    }

    private Comment HandleComment()
    {
        while (!IsAtEnd && Peek() != '\n')
            Advance();

        return new();
    }

    private Whitespace HandleWhitespace()
    {
        while (!IsAtEnd && Peek() is ' ' or '\r' or '\t')
            Advance();

        return new();
    }

    private Whitespace HandleLinebreak()
    {
        line++;
        return new();
    }
}