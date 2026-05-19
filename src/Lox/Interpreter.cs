namespace Lox;

public class Interpreter
{
    public bool HadError { get; private set; }

    public async Task RunFileAsync(string path)
    {
        var source = await File.ReadAllTextAsync(path);
        Run(source);
    }

    public void RunPrompt()
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

    static private void Run(string source)
    {
        var lexer = new Lexer();

        foreach (var token in lexer.Lex(source))
            Console.WriteLine(token);
    }

    private void ReportError(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }
}