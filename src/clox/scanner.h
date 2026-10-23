#pragma once

#include "tokenType.h"
#include <stddef.h>
#include <stdint.h>

typedef struct {
  TokenType type;
  const char* start;
  size_t length;
  size_t line;
} Token;

void initScanner(const char* source);
Token scanToken();
