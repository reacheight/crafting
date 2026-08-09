this is me following along Robert Nystrom's book "Crafting Interpreters" in C# (tree-walk interpreter implementation).

apart from learning about programming languages in this project I also try to experiment and exercise with C# 15 unions, functional programming elements in C#, and "parse, don't validate" / type-driven design.

## diff with the book implementation

- something is probably broken
- explicitly defined AST classes instead of code generation
- no visitor pattern, just pattern matching
- some challenges done in the main branch (ternary operator, string + non-string concatenation, break statements, lambdas, unused variable warnings)
- pattern matching support via `is` and `switch` expression
- static checks that can be detected syntactically ('return' outside function, 'this'/'super' outside class) are done in the parser, not resolver, because I like to report them as early as possible

### pattern matching

#### is-expression

`expr is pattern`, evaluates to bool value

#### switch-expression

```
expr switch {
    pattern1: onMatchExpr1,
    pattern2: onMatchExpr2,
}
```

evaluates to the first matched pattern's onMatchExpr. if no pattern is matched, evaluates to nil.

#### supported patterns

- literals (`expr is "lol"`, `expr is 23`, `expr is false`)
- nil (`expr is nil`)
- built-in types (`expr is str`, `expr is num`, `expr is bool`)
- declared types, supports class hierarchy (`expr is MyClass`)
- discard, always matches (`expr is _`)

#### example

```
class Animal {
  init(name) { this.name = name; }
}

class Dog < Animal {
  init(name) { super.init(name); }
}

class Cat < Animal {
  init(name) { super.init(name); }
}

var dog = Dog("Julie");

if (dog is Dog) {
    print "dog is Dog";
}

if (dog.name is "Julie") {
    print "dog is Julie";
}

if (dog is Animal) {
    print "dog is Animal";
}

var kind = dog switch {
    22: "kind of 22",
    "hello": "kind of hello",
    str: "string",
    num: "number",
    Cat: "cat",
    Dog: "dog",
    _: "unknown"
};
print kind; // "dog"
```

## how to build and run

requires .net 11 preview 5 or higher

- `dotnet run --project src/Lox -- path/to/script.lox` to run a file
- `dotnet run --project src/Lox` to run REPL
