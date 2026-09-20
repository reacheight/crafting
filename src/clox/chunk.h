#pragma once

#include "value.h"

typedef enum { OP_CONSTANT, OP_RETURN } OpCode;

typedef struct {
    size_t count;
    size_t capacity;
    uint8_t* code;
    size_t* lines;
    ValueArray constants;
} Chunk;

void initChunk(Chunk* chunk);
void writeChunk(Chunk* chunk, uint8_t byte, size_t line);
void freeChunk(Chunk* chunk);

int addConstant(Chunk* chunk, Value value);
