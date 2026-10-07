#include "chunk.h"
#include "vm.h"
#include <stdint.h>
#include <stdlib.h>

int main(int argc, const char* argv[argc]) {
    initVM();

    Chunk chunk;
    initChunk(&chunk);

    writeConstant(&chunk, 4.1, 1);
    writeChunk(&chunk, OP_NEGATE, 2);
    writeChunk(&chunk, OP_RETURN, 3);

    interpret(&chunk);

    freeChunk(&chunk);
    freeVM();

    return EXIT_SUCCESS;
}
