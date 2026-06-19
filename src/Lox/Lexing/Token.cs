namespace Lox.Lexing;

public record Token(SourceLocation Location, TokenType Type);

public record struct SourceLocation(int Line);

public readonly union TokenType(
    LeftParen, RightParen, LeftBrace, RightBrace,
    Colon, Comma, Semicolon, Dot,
    Minus, Plus, Slash, Star, Question,
    Bang, BangEqual, Equal, EqualEqual,
    Greater, GreaterEqual, Less, LessEqual,
    Identifier, Var, And, Or, Class, Fun,
    This, Super, If, Else, True, False, Nil,
    Print, Return, For, While, Break,
    LiteralToken,
    Eof
);

public record struct LeftParen;
public record struct RightParen;

public record struct LeftBrace;
public record struct RightBrace;

public record struct Colon;
public record struct Comma;
public record struct Semicolon;
public record struct Dot;

public record struct Minus;
public record struct Plus;
public record struct Slash;
public record struct Star;
public record struct Question;

public record struct Bang;
public record struct BangEqual;
public record struct Equal;
public record struct EqualEqual;
public record struct Greater;
public record struct GreaterEqual;
public record struct Less;
public record struct LessEqual;

public record Identifier(string Name);
public record struct Var;

public record struct And;
public record struct Or;

public record struct Class;
public record struct Fun;
public record struct This;
public record struct Super;

public record struct If;
public record struct Else;

public record struct True;
public record struct False;
public record struct Nil;

public record struct Print;
public record struct Return;

public record struct For;
public record struct While;
public record struct Break;

public record struct Eof;

public readonly union LiteralToken(StringLiteralToken, NumberLiterlToken);
public record StringLiteralToken(string Value);
public record struct NumberLiterlToken(double Value);