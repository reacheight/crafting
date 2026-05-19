namespace Lox;

public class Lexer
{
    public IEnumerable<string> Lex(string source)
    {
        return source.Split(" ");
    }
}