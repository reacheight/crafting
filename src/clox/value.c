#include "value.h"
#include "memory.h"
#include "object.h"
#include <stdio.h>
#include <string.h>

void initValueArray(ValueArray* array) {
  array->count = 0;
  array->capacity = 0;
  array->values = nullptr;
}

void writeValueArray(ValueArray* array, Value value) {
  if (array->capacity < array->count + 1) {
    size_t oldCapacity = array->capacity;
    array->capacity = grow_capacity(oldCapacity);
    array->values = GROW_ARRAY(Value, array->values, oldCapacity, array->capacity);
  }

  array->values[array->count] = value;
  array->count++;
}

void freeValueArray(ValueArray* array) {
  FREE_ARRAY(Value, array->values, array->capacity);
  initValueArray(array);
}

void printValue(Value value) {
  switch (value.type) {
    case VAL_NUMBER: {
      printf("%g", asNumber(value));
      break;
    }
    case VAL_BOOL: {
      printf(asBool(value) ? "true" : "false");
      break;
    }
    case VAL_NIL: {
      printf("nil");
      break;
    }
    case VAL_OBJ: {
      printObject(value);
      break;
    }
  }
}

bool valuesEqual(Value a, Value b) {
  if (a.type != b.type)
    return false;

  switch (a.type) {
    case VAL_BOOL:
      return asBool(a) == asBool(b);
    case VAL_NUMBER:
      return asNumber(a) == asNumber(b);
    case VAL_NIL:
      return true;
    case VAL_OBJ: {
      ObjString* aString = asString(a);
      ObjString* bString = asString(b);
      return aString->length == bString->length &&
             memcmp(aString->chars, bString->chars, aString->length) == 0;
    }
    default:
      return false;
  }
}

Value fromNumber(double val) {
  return (Value){
    .type = VAL_NUMBER,
    .as = { .number = val },
  };
}

Value fromBool(bool val) {
  return (Value){
    .type = VAL_BOOL,
    .as = { .boolean = val },
  };
}

Value fromObj(Obj* obj) {
  return (Value){
    .type = VAL_OBJ,
    .as = { .obj = obj },
  };
}

double asNumber(Value val) {
  return val.as.number;
}

bool asBool(Value val) {
  return val.as.boolean;
}

Obj* asObj(Value val) {
  return val.as.obj;
}

bool isBool(Value val) {
  return val.type == VAL_BOOL;
}

bool isNumber(Value val) {
  return val.type == VAL_NUMBER;
}

bool isNil(Value val) {
  return val.type == VAL_NIL;
}

bool isObj(Value val) {
  return val.type == VAL_OBJ;
}

bool isFalsey(Value val) {
  return isNil(val) || (isBool(val) && !asBool(val));
}
