using Lox;

if (args.Length > 1)
{
    Console.WriteLine("Usage: lox [script]");
    return;
}

var interpreter = new Interpreter();

if (args is [var path])
    await interpreter.RunFileAsync(path);
else
    interpreter.RunPrompt();