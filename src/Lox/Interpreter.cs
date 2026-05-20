using Lox.Lexing;

namespace Lox;

public static class Interpreter
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

    private static void Run(string source)
    {
        var lexer = new Lexer(source);

        foreach (var token in lexer.LexTokens())
            Console.WriteLine(token);
    }

    private static void ReportError(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }
}