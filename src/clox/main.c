#include "chunk.h"
#include "vm.h"
#include <stdint.h>
#include <stdlib.h>

int main(int argc, const char* argv[argc]) {
    initVM();

    Chunk chunk;
    initChunk(&chunk);

    for (uint16_t i = 0; i < 312; i++)
        writeConstant(&chunk, i, i);
    writeChunk(&chunk, OP_RETURN, 3);

    interpret(&chunk);

    freeChunk(&chunk);
    freeVM();

    return EXIT_SUCCESS;
}
