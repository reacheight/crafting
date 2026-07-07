using Lox.Interpreting;
using Lox.Interpreting.Globals;
using Lox.Lexing;
using Lox.Parsing;

namespace Lox;

public static class Runner
{
    private static bool HadSyntaxError { get; set; }
    private static bool HadRuntimeError { get; set; }

    private static readonly Interpreter interpreter = new(new()
    {
        ["clock"] = new Clock(),
    });

    public static async Task RunFileAsync(string path)
    {
        var source = await File.ReadAllTextAsync(path);
        Run(source);

        if (HadSyntaxError)
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
            HadSyntaxError = false;
        }
    }

    private static void Run(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Parser([.. tokens]);
        var parseResult = parser.Parse();

        if (HadSyntaxError || parseResult is not LoxProgram program)
            return;

        interpreter.Interpret(program);
    }

    public static void ReportRuntimeError(RuntimeException error)
    {
        Console.Error.WriteLine($"[line {error.Location.Line}] RuntimeError: {error.Message}");
        HadRuntimeError = true;
    }

    public static void ReportError(SourceLocation location, string message)
    {
        Console.Error.WriteLine($"[line {location.Line}] SyntaxError: {message}");
        HadSyntaxError = true;
    }
}