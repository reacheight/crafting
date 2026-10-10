#include "scanner.h"
#include <ctype.h>
#include <stdint.h>
#include <string.h>

typedef struct {
    const char* start;
    const char* current;
    int line;
} Scanner;

Scanner scanner;

void initScanner(const char* source) {
    scanner.start = source;
    scanner.current = source;
    scanner.line = 1;
}

static bool isAtEnd() {
    return *scanner.current == '\0';
}

char peek() {
    return *scanner.current;
}

char peekNext() {
    if (isAtEnd())
        return '\0';

    return *(scanner.current + 1);
}

char advance() {
    auto current = *scanner.current;

    if (!isAtEnd())
        scanner.current++;

    return current;
}

bool match(char expected) {
    if (isAtEnd())
        return false;

    if (*scanner.current != expected)
        return false;

    scanner.current++;
    return true;
}

Token makeToken(TokenType type) {
    Token token = {
        .type = type,
        .line = scanner.line,
        .start = scanner.start,
        .length = scanner.current - scanner.start,
    };

    return token;
}

Token errorToken(const char message[UINT8_MAX]) {
    Token token = {
        .type = TOKEN_ERROR,
        .line = scanner.line,
        .start = message,
        .length = (uint8_t)strlen(message),
    };

    return token;
}

static void skipWhitespace() {
    while (true) {
        switch (peek()) {
            case ' ':
            case '\r':
            case '\t':
                advance();
                break;
            case '\n':
                scanner.line++;
                advance();
                break;
            case '/':
                if (peekNext() == '/') {
                    while (peek() != '\n' && !isAtEnd())
                        advance();
                    break;
                } else {
                    return;
                }
            default:
                return;
        }
    }
}

Token number() {
    while (isdigit(peek()))
        advance();

    if (peek() == '.' && isdigit(peekNext())) {
        advance();

        while (isdigit(peek()))
            advance();
    }

    return makeToken(TOKEN_NUMBER);
}

Token string() {
    while (peek() != '"' && !isAtEnd()) {
        if (peek() == '\n')
            scanner.line++;

        advance();
    }

    if (isAtEnd())
        return errorToken("Unterminated string.");

    advance();
    return makeToken(TOKEN_STRING);
}

static bool canBeInIdentifier(char c) {
    return isalpha(c) || c == '_';
}

TokenType identifierType() {
    return TOKEN_IDENTIFIER;
}

Token identifier() {
    while (canBeInIdentifier(peek()) || isdigit(peek()))
        advance();

    return makeToken(identifierType());
}

Token scanToken() {
    skipWhitespace();
    scanner.start = scanner.current;

    if (isAtEnd())
        return makeToken(TOKEN_EOF);

    auto c = advance();

    if (canBeInIdentifier(c))
        return identifier();

    if (isdigit(c))
        return number();

    switch (c) {
        case '(':
            return makeToken(TOKEN_LEFT_PAREN);
        case ')':
            return makeToken(TOKEN_RIGHT_PAREN);
        case '{':
            return makeToken(TOKEN_LEFT_BRACE);
        case '}':
            return makeToken(TOKEN_RIGHT_BRACE);
        case ';':
            return makeToken(TOKEN_SEMICOLON);
        case ',':
            return makeToken(TOKEN_COMMA);
        case '.':
            return makeToken(TOKEN_DOT);
        case '-':
            return makeToken(TOKEN_MINUS);
        case '+':
            return makeToken(TOKEN_PLUS);
        case '/':
            return makeToken(TOKEN_SLASH);
        case '*':
            return makeToken(TOKEN_STAR);
        case '!':
            return makeToken(match('=') ? TOKEN_BANG_EQUAL : TOKEN_BANG);
        case '=':
            return makeToken(match('=') ? TOKEN_EQUAL_EQUAL : TOKEN_EQUAL);
        case '>':
            return makeToken(match('=') ? TOKEN_GREATER_EQUAL : TOKEN_GREATER);
        case '<':
            return makeToken(match('=') ? TOKEN_LESS_EQUAL : TOKEN_LESS);
        case '"':
            return string();
    }

    return errorToken("Unexptected character.");
}
