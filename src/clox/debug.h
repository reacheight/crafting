#pragma once

#include "chunk.h"
#include <stddef.h>

void disassembleChunk(Chunk* chunk, const char* name);
int disassembleInstruction(Chunk* chunk, size_t offset);
