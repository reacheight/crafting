#pragma once

#include "value.h"

typedef enum {
  OBJ_STRING,
} ObjType;

struct Obj {
  ObjType type;
  Obj* next;
};

struct ObjString {
  Obj obj;
  int length;
  const char* chars;
};

ObjString* asString(Value value);
bool isString(Value value);

ObjString* copyString(const char* chars, size_t length);
ObjString* takeString(const char* chars, size_t length);
void printObject(Value value);
