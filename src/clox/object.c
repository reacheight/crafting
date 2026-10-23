#include "object.h"
#include "memory.h"
#include "value.h"
#include <stddef.h>
#include <stdio.h>
#include <string.h>

#define ALLOCATE_OBJ(type, objType) (type*)allocateObject(sizeof(type), objType)

static ObjType getObjType(Value value) {
  return asObj(value)->type;
}

static bool isObjType(Value value, ObjType type) {
  return isObj(value) && getObjType(value) == type;
}

static bool isString(Value value) {
  return isObjType(value, OBJ_STRING);
}

ObjString* asString(Value value) {
  return (ObjString*)asObj(value);
}

static const char* asCString(Value value) {
  return asString(value)->chars;
}

static Obj* allocateObject(size_t size, ObjType objType) {
  Obj* obj = reallocate(nullptr, 0, size);
  obj->type = objType;

  return obj;
}

static ObjString* allocateString(const char* chars, size_t length) {
  ObjString* string = ALLOCATE_OBJ(ObjString, OBJ_STRING);
  string->chars = chars;
  string->length = length;

  return string;
}

ObjString* copyString(const char* chars, size_t length) {
  auto heapChars = ALLOCATE(char, length + 1);
  memcpy(heapChars, chars, length);
  heapChars[length] = '\0';

  return allocateString(heapChars, length);
}

void printObject(Value value) {
  switch (getObjType(value)) {
    case OBJ_STRING:
      printf("%s", asCString(value));
      break;
  }
}
