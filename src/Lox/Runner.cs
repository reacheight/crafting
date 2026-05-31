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
            if (input is null or "exit")
                break;

            Run(input);
            HadError = false;
        }
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

    public static void ReportError(int line, string message)
        => ReportError(line, "", message);

    public static void ReportError(Token token, string message)
    {
        if (token.Type is NonLiteralTokenType.Eof)
            ReportError(token.Line, " at end", message);
        else
            ReportError(token.Line, $" at '{token.Lexeme}'", message);
    }

    private static void ReportError(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}