this is me following along Robert Nystrom's book "Crafting Interpreters" in C# (tree-walk interpreter implementation).

apart from learning about programming languages in this project I also try to experiment and exercise with C# 15 unions, functional programming elements in C#, and "parse, don't validate" / type-driven design.

## diff with the book implementation

- something is probably broken
- explicitly defined AST classes instead of code generation
- no visitor pattern, just pattern matching
- some challenges done in the main branch (ternary operator, string + non-string concatenation, break statements, lambdas, unused variable warnings)
- static checks that can be detected syntactically ('return' outside function, 'this'/'super' outside class) are done in the parser, not resolver, because I like to report them as early as possible

## how to build and run

requires .net 11 preview 5 or higher

- `dotnet run --project src/Lox -- path/to/script.lox` to run a file
- `dotnet run --project src/Lox` to run REPL
