#include "scanner.h"
#include <ctype.h>
#include <stddef.h>
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

static char peek() {
  return *scanner.current;
}

static char peekNext() {
  if (isAtEnd())
    return '\0';

  return *(scanner.current + 1);
}

static char advance() {
  auto current = *scanner.current;

  if (!isAtEnd())
    scanner.current++;

  return current;
}

static bool match(char expected) {
  if (isAtEnd())
    return false;

  if (*scanner.current != expected)
    return false;

  scanner.current++;
  return true;
}

static size_t lexemeLength() {
  return scanner.current - scanner.start;
}

static Token makeToken(TokenType type) {
  Token token = {
    .type = type,
    .line = scanner.line,
    .start = scanner.start,
    .length = lexemeLength(),
  };

  return token;
}

static Token errorToken(const char* message) {
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

static Token number() {
  while (isdigit(peek()))
    advance();

  if (peek() == '.' && isdigit(peekNext())) {
    advance();

    while (isdigit(peek()))
      advance();
  }

  return makeToken(TOKEN_NUMBER);
}

static Token string() {
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

static TokenType checkKeyword(size_t checkedLen, size_t restLen, const char* rest, TokenType type) {
  auto keywordLength = checkedLen + restLen;
  if (lexemeLength() == keywordLength && memcmp(scanner.start + checkedLen, rest, restLen) == 0)
    return type;

  return TOKEN_IDENTIFIER;
}

static TokenType identifierType() {
  switch (scanner.start[0]) {
    case 'a':
      return checkKeyword(1, 2, "nd", TOKEN_AND);
    case 'c':
      return checkKeyword(1, 4, "lass", TOKEN_CLASS);
    case 'e':
      return checkKeyword(1, 3, "lse", TOKEN_ELSE);
    case 'i':
      return checkKeyword(1, 1, "f", TOKEN_IF);
    case 'n':
      return checkKeyword(1, 2, "il", TOKEN_NIL);
    case 'o':
      return checkKeyword(1, 1, "r", TOKEN_OR);
    case 'p':
      return checkKeyword(1, 4, "rint", TOKEN_PRINT);
    case 'r':
      return checkKeyword(1, 5, "eturn", TOKEN_RETURN);
    case 's':
      return checkKeyword(1, 4, "uper", TOKEN_SUPER);
    case 'v':
      return checkKeyword(1, 2, "ar", TOKEN_VAR);
    case 'w':
      return checkKeyword(1, 4, "hile", TOKEN_WHILE);
    case 'f':
      if (lexemeLength() > 1) {
        switch (scanner.start[1]) {
          case 'a':
            return checkKeyword(2, 3, "lse", TOKEN_FALSE);
          case 'o':
            return checkKeyword(2, 1, "r", TOKEN_FOR);
          case 'u':
            return checkKeyword(2, 1, "n", TOKEN_FUN);
        }
      }
      break;
    case 't':
      if (scanner.current - scanner.start > 1) {
        switch (scanner.start[1]) {
          case 'h':
            return checkKeyword(2, 2, "is", TOKEN_THIS);
          case 'r':
            return checkKeyword(2, 2, "ue", TOKEN_TRUE);
        }
      }
      break;
  }
  return TOKEN_IDENTIFIER;
}

static Token identifier() {
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
