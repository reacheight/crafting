#include <stddef.h>
#include <stdlib.h>

#include "memory.h"

size_t grow_capacity(size_t capacity) {
    return capacity < 8 ? 8 : capacity * 2;
}

void* reallocate(void* pointer, size_t oldSize, size_t newSize) {
    if (newSize == 0) {
        free(pointer);
        return nullptr;
    }

    auto result = realloc(pointer, newSize);
    if (!result)
        exit(1);

    return result;
}
