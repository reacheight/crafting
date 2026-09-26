#pragma once

#include <stddef.h>

#define GROW_ARRAY(type, pointer, oldCount, newCount)                                                                  \
    (type*)reallocate(pointer, sizeof(type) * (oldCount), sizeof(type) * (newCount));

#define FREE_ARRAY(type, pointer, oldCount) (type*)reallocate(pointer, sizeof(type) * (oldCount), 0)

size_t grow_capacity(size_t capacity);
void* reallocate(void* pointer, size_t oldSize, size_t newSize);
