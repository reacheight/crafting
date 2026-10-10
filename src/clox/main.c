#include "vm.h"
#include <stdio.h>
#include <stdlib.h>

static void repl() {
    char line[1024];
    while (true) {
        printf("> ");

        if (!fgets(line, sizeof(line), stdin)) {
            printf("\n");
            break;
        }

        interpret(line);
    }
}

static char* readFile(const char* path) {
    auto file = fopen(path, "rb");
    if (!file) {
        fprintf(stderr, "Could not open file \"%s\".\n", path);
        exit(74);
    }

    fseek(file, 0, SEEK_END);
    size_t fileSize = ftell(file);
    rewind(file);

    char* fileString = malloc(fileSize + 1);
    if (!fileString) {
        fprintf(stderr, "Not enough memory to read file \"%s\"", path);
    }

    auto bytesRead = fread(fileString, sizeof(char), fileSize, file);
    if (bytesRead < fileSize) {
        fprintf(stderr, "Could not read file \"%s\"", path);
    }

    fclose(file);

    fileString[bytesRead] = '\n';
    return fileString;
}

static void runFile(const char* path) {
    auto source = readFile(path);
    auto result = interpret(source);
    free(source);

    if (result == INTERPRET_COMPILE_ERROR)
        exit(65);

    if (result == INTERPRET_RUNTIME_ERROR)
        exit(70);
}

int main(int argc, const char* argv[argc]) {
    initVM();

    if (argc == 1) {

    } else if (argc == 2) {

    } else {
        fprintf(stderr, "Usage: clox [script]\n");
    }

    freeVM();
    return EXIT_SUCCESS;
}
