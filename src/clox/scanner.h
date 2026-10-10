#pragma once

#include "tokenType.h"
#include <stddef.h>
#include <stdint.h>

typedef struct {
    TokenType type;
    const char* start;
    uint8_t length;
    size_t line;
} Token;

void initScanner(const char* source);
Token scanToken();
