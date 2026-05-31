using Lox.Lexing;
using Lox.Parsing;

namespace Lox;

public static class Runner
{
    public static bool HadError { get; private set; }

    public static async Task RunFileAsync(string path)
    {
        var source = await File.ReadAllTextAsync(path);
        Run(source);
    }

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");

            var input = Console.ReadLine();
            if (input is null)
                return;

            Run(input);
        }
    }

    public static void Error(int line, string message)
        => ReportError(line, "", message);

    public static void Error(Token token, string message)
    {
        if (token.Type is NonLiteralTokenType.Eof)
            ReportError(token.Line, " at end", message);
        else
            ReportError(token.Line, $" at '{token.Lexeme}'", message);
    }

    private static void Run(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Parser([.. tokens]);
        var expr = parser.Parse();

        if (HadError)
            return;

        if (expr is not null)
            Console.WriteLine(expr);
    }

    private static void ReportError(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}