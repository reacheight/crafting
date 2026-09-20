#include "chunk.h"
#include "debug.h"
#include <stdlib.h>

int main(int argc, const char* argv[argc]) {
    Chunk chunk;
    initChunk(&chunk);

    int constant_idx = addConstant(&chunk, 3.4);
    writeChunk(&chunk, OP_CONSTANT, 2);
    writeChunk(&chunk, constant_idx, 2);

    int constant2_idx = addConstant(&chunk, 0);
    writeChunk(&chunk, OP_CONSTANT, 3);
    writeChunk(&chunk, constant2_idx, 3);

    writeChunk(&chunk, OP_RETURN, 3);

    disassembleChunk(&chunk, "testing");

    freeChunk(&chunk);

    return EXIT_SUCCESS;
}
