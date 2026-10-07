#include "chunk.h"
#include "vm.h"
#include <stdint.h>
#include <stdlib.h>

int main(int argc, const char* argv[argc]) {
    initVM();

    Chunk chunk;
    initChunk(&chunk);

    writeConstant(&chunk, 1.2, 1);
    writeConstant(&chunk, 3.8, 1);
    writeChunk(&chunk, OP_ADD, 1);

    writeConstant(&chunk, 2.5, 2);
    writeChunk(&chunk, OP_DIVIDE, 2);
    writeChunk(&chunk, OP_NEGATE, 2);

    writeChunk(&chunk, OP_RETURN, 3);

    interpret(&chunk);

    freeChunk(&chunk);
    freeVM();

    return EXIT_SUCCESS;
}
