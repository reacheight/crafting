#include "compiler.h"
#include "scanner.h"
#include <stddef.h>
#include <stdio.h>

void compile(const char* source) {
    initScanner(source);
    size_t line = 0;

    while (true) {
        auto token = scanToken();
        if (token.line != line) {
            printf("%4zu ", token.line);
            line = token.line;
        } else {
            printf("   | ");
        }

        printf("%2d '%.*s'\n", token.type, token.length, token.start);

        if (token.type == TOKEN_EOF)
            break;
    }
}
