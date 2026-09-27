#include "chunk.h"
#include "debug.h"
#include <stdlib.h>

int main(int argc, const char* argv[argc]) {
    Chunk chunk;
    initChunk(&chunk);

    for (auto i = 0; i < 712; i++) {
        writeConstant(&chunk, i, i);
    }

    writeChunk(&chunk, OP_RETURN, 3);

    disassembleChunk(&chunk, "testing");

    freeChunk(&chunk);

    return EXIT_SUCCESS;
}
