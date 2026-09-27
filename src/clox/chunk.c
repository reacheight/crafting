#include "chunk.h"
#include "memory.h"
#include <stddef.h>
#include <stdint.h>

void initChunk(Chunk* chunk) {
    chunk->count = 0;
    chunk->capacity = 0;
    chunk->code = nullptr;
    chunk->lines = nullptr;
    initValueArray(&chunk->constants);
}

void writeChunk(Chunk* chunk, uint8_t byte, size_t line) {
    if (chunk->capacity < chunk->count + 1) {
        size_t oldCapacity = chunk->capacity;
        chunk->capacity = grow_capacity(oldCapacity);
        chunk->code = GROW_ARRAY(uint8_t, chunk->code, oldCapacity, chunk->capacity);
        chunk->lines = GROW_ARRAY(size_t, chunk->lines, oldCapacity, chunk->capacity);
    }

    chunk->code[chunk->count] = byte;
    chunk->lines[chunk->count] = line;
    chunk->count++;
}

void writeConstant(Chunk* chunk, Value value, size_t line) {
    writeValueArray(&chunk->constants, value);
    auto const_idx = chunk->constants.count - 1;

    if (const_idx <= UINT8_MAX) {
        writeChunk(chunk, OP_CONSTANT, line);
        writeChunk(chunk, const_idx, line);
    } else {
        writeChunk(chunk, OP_CONSTANT_LONG, line);
        writeChunk(chunk, const_idx & 255, line);
        writeChunk(chunk, (const_idx >> 8) & 255, line);
        writeChunk(chunk, (const_idx >> 16) & 255, line);
    }
}

void freeChunk(Chunk* chunk) {
    FREE_ARRAY(uint8_t, chunk->code, chunk->capacity);
    FREE_ARRAY(size_t, chunk->lines, chunk->capacity);
    freeValueArray(&chunk->constants);
    initChunk(chunk);
}

int addConstant(Chunk* chunk, Value value) {
    writeValueArray(&chunk->constants, value);
    return chunk->constants.count - 1;
}
