using Lox;

if (args.Length > 1)
{
    Console.WriteLine("Usage: lox [script]");
    return;
}

if (args is [var path])
    await Runner.RunFileAsync(path);
else
    Runner.RunPrompt();