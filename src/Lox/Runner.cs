using Lox.Interpreting;
using Lox.Lexing;
using Lox.Parsing;
using Lox.Resolving;

namespace Lox;

public static class Runner
{
    private static bool HadStaticError { get; set; }
    private static bool HadRuntimeError { get; set; }

    private static readonly Interpreter interpreter = new();

    public static async Task RunFileAsync(string path)
    {
        var source = await File.ReadAllTextAsync(path);
        Run(source);

        if (HadStaticError)
            System.Environment.Exit(65);

        if (HadRuntimeError)
            System.Environment.Exit(70);
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
            HadStaticError = false;
        }
    }

    private static void Run(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Parser([.. tokens]);
        var statements = parser.Parse();

        if (HadStaticError)
            return;

        var resolver = new Resolver(interpreter);
        resolver.Resolve(statements);

        if (HadStaticError)
            return;

        interpreter.Interpret(statements);
    }

    public static void ReportRuntimeError(RuntimeException error)
    {
        Console.Error.WriteLine($"[line {error.Location.Line}] RuntimeError: {error.Message}");
        HadRuntimeError = true;
    }

    public static void ReportError(SourceLocation location, string message)
    {
        Console.Error.WriteLine($"[line {location.Line}] SyntaxError: {message}");
        HadStaticError = true;
    }

    public static void ReportWarn(SourceLocation location, string message)
    {
        Console.Error.WriteLine($"[line {location.Line}] Warning: {message}");
    }
}