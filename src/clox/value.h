#pragma once

#include "common.h"

typedef enum { VAL_NIL, VAL_BOOL, VAL_NUMBER } ValueType;

typedef struct {
  ValueType type;

  union {
    bool boolean;
    double number;
  } as;
} Value;

typedef struct {
  size_t capacity;
  size_t count;
  Value* values;
} ValueArray;

void initValueArray(ValueArray* array);
void writeValueArray(ValueArray* array, Value value);
void freeValueArray(ValueArray* array);
void printValue(Value value);

// Value NIL_VAL = {
//   .type = VAL_NIL,
//   .as = { .number = 0 },
// };
#define NIL_VAL ((Value){ VAL_NIL, { .number = 0 } })

Value from_number(double val);
Value from_bool(bool val);

double as_number(Value val);
bool as_bool(Value val);

bool is_bool(Value val);
bool is_number(Value val);
bool is_nil(Value val);
