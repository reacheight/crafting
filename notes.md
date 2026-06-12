## 6 parsing

kinda don't like this grammar rework that helps to define precedence levels.
it just looks odd for me that, for example, equality can consist of only comparison without equality operator and so on.

## 8 statements and state

(unrelated to book content) don't like how my expr / stmt ASTs look now. i tried to define types for expressions so they couldn't represent invalid states
i. e. made separate types for binary and unary operators instead of storing raw tokens, so it couldn't be possible at compile time to create a binary expression with some wrong token stored as an operator. but still ended up storing a token for an operator for error reporting. statements also need tokens for identifier lexems to define variables and get their values. want to refactor my parsing types.

also i'm not sure how i feel about this little Unit type defined in Interpreter. i made it so i could write a proper switch expression for statement executing -- each branch returns an instance of this Unit type. i also just could to use discard assignment like

```csharp
private void Execute(Stmt stmt) => _ = stmt switch
{
    ExprStmt exprStmt => ExecuteExprStmt(exprStmt),
};
```

not sure what i prefer.

the book itslef is really nice, i really like this approach when we build a fully working but really small subset of the language first and then enrich it step by step.
