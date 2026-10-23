#pragma once

#include "common.h"

typedef struct Obj Obj;
typedef struct ObjString ObjString;

typedef enum {
  VAL_NIL,
  VAL_BOOL,
  VAL_NUMBER,
  VAL_OBJ,
} ValueType;

typedef struct {
  ValueType type;

  union {
    bool boolean;
    double number;
    Obj* obj;
  } as;
} Value;

typedef struct {
  size_t capacity;
  size_t count;
  Value* values;
} ValueArray;

bool valuesEqual(Value a, Value b);
void initValueArray(ValueArray* array);
void writeValueArray(ValueArray* array, Value value);
void freeValueArray(ValueArray* array);
void printValue(Value value);

// Value NIL_VAL = {
//   .type = VAL_NIL,
//   .as = { .number = 0 },
// };
#define NIL_VAL ((Value){ VAL_NIL, { .number = 0 } })

Value fromNumber(double val);
Value fromBool(bool val);
Value fromObj(Obj* obj);

double asNumber(Value val);
bool asBool(Value val);
Obj* asObj(Value val);

bool isBool(Value val);
bool isNumber(Value val);
bool isNil(Value val);
bool isObj(Value val);
bool isFalsey(Value val);
