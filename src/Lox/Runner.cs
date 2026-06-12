using Lox.Interpreting;
using Lox.Lexing;
using Lox.Parsing;

namespace Lox;

public static class Runner
{
    private static bool HadSyntaxError { get; set; }
    private static bool HadRuntimeError { get; set; }

    private static readonly Interpreter interpreter = new();

    public static async Task RunFileAsync(string path)
    {
        var source = await File.ReadAllTextAsync(path);
        Run(source);

        if (HadSyntaxError)
            Environment.Exit(65);

        if (HadRuntimeError)
            Environment.Exit(70);
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
            HadSyntaxError = false;
        }
    }

    private static void Run(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Parser([.. tokens]);
        var parseResult = parser.Parse();

        if (HadSyntaxError || parseResult is not Expr expr)
            return;

        interpreter.Interpret(expr);
    }

    public static void ReportError(int line, string message)
        => ReportError(line, "", message);

    public static void ReportError(Token token, string message)
    {
        if (token.Type is Eof)
            ReportError(token.Line, " at end", message);
        else
            ReportError(token.Line, $" at '{token.Lexeme}'", message);
    }

    public static void ReportRuntimeError(RuntimeException error)
    {
        Console.Error.WriteLine($"[line {error.Token.Line}] RuntimeError: {error.Message}");
        HadRuntimeError = true;
    }

    private static void ReportError(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] SyntaxError{where}: {message}");
        HadSyntaxError = true;
    }
}