using Lox;

if (args.Length > 1)
{
    Console.WriteLine("Usage: lox [script]");
    return;
}

if (args is [var path])
    await Interpreter.RunFileAsync(path);
else
    Interpreter.RunPrompt();