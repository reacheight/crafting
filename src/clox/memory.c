#include <stddef.h>
#include <stdlib.h>

#include "memory.h"
#include "object.h"
#include "value.h"
#include "vm.h"

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

static void freeObject(Obj* object) {
  switch (object->type) {
    case OBJ_STRING: {
      auto string = (ObjString*)object;
      FREE_ARRAY(char, (char*)string->chars, string->length + 1);
      FREE(ObjString, object);
      break;
    }
  }
}

void freeObjects() {
  auto current = vm.objects;
  while (current->next != nullptr) {
    auto next = current->next;
    freeObject(current);
    current = next;
  }
}
