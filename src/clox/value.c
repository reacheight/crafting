#include "value.h"
#include "memory.h"
#include <stdio.h>

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
  printf("%g", as_number(value));
}

Value from_number(double val) {
  return (Value){
    .type = VAL_NUMBER,
    .as = { .number = val },
  };
}

Value from_bool(bool val) {
  return (Value){
    .type = VAL_BOOL,
    .as = { .boolean = val },
  };
}

double as_number(Value val) {
  return val.as.number;
}

bool as_bool(Value val) {
  return val.as.boolean;
}

bool is_bool(Value val) {
  return val.type == VAL_BOOL;
}

bool is_number(Value val) {
  return val.type == VAL_NUMBER;
}

bool is_nil(Value val) {
  return val.type == VAL_NIL;
}
