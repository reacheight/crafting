#include "vm.h"
#include "chunk.h"
#include "compiler.h"
#include "debug.h"
#include "value.h"
#include <stdarg.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>

VM vm;

static void resetStack() {
  vm.stackTop = vm.stack;
}

static void runtimeError(const char* format, ...) {
  va_list args;
  va_start(args, format);
  vfprintf(stderr, format, args);
  va_end(args);
  fputs("\n", stderr);

  auto instruction = vm.ip - vm.chunk->code - 1;
  auto line = vm.chunk->lines[instruction];
  fprintf(stderr, "[line %zu] in script\n", line);
  resetStack();
}

void initVM() {
  resetStack();
}

void freeVM() {}

void push(Value value) {
  *vm.stackTop = value;
  vm.stackTop++;
}

Value pop() {
  vm.stackTop--;
  return *vm.stackTop;
}

static Value peek(size_t distance) {
  return vm.stackTop[-1 - distance];
}

static uint8_t readByte() {
  // save current ip, increment vm.ip, dereference and return the saved pointer
  return *vm.ip++;
}

static Value readConstant(size_t operandCount) {
  uint32_t constant_idx = readByte();
  for (auto i = 1; i < operandCount; i++) {
    constant_idx |= readByte() << (8 * i);
  }

  return vm.chunk->constants.values[constant_idx];
}

static InterpreterResult run() {
#define BINARY_OP(toValue, op)                                                                     \
  do {                                                                                             \
    if (!isNumber(peek(0)) || !isNumber(peek(1))) {                                                \
      runtimeError("Operands must be numbers.");                                                   \
      return INTERPRET_RUNTIME_ERROR;                                                              \
    }                                                                                              \
    double b = asNumber(pop());                                                                    \
    double a = asNumber(pop());                                                                    \
    push(toValue(a op b));                                                                         \
  } while (false)

  while (true) {
#ifdef DEBUG_TRACING_EXECUTION
    printf("          ");
    for (auto slot = vm.stack; slot < vm.stackTop; slot++) {
      printf("[ ");
      printValue(*slot);
      printf(" ]");
    }
    printf("\n");
    disassembleInstruction(vm.chunk, (int)(vm.ip - vm.chunk->code));
#endif
    auto instruction = readByte();
    switch (instruction) {
      case OP_CONSTANT: {
        auto constant = readConstant(1);
        push(constant);
        break;
      }
      case OP_CONSTANT_LONG: {
        auto constant = readConstant(3);
        push(constant);
        break;
      }
      case OP_TRUE: {
        push(fromBool(true));
        break;
      }
      case OP_FALSE: {
        push(fromBool(false));
        break;
      }
      case OP_NIL: {
        push(NIL_VAL);
        break;
      }
      case OP_NOT: {
        push(fromBool(isFalsey(pop())));
        break;
      }
      case OP_NEGATE: {
        if (!isNumber(peek(0))) {
          runtimeError("Operand must be a number.");
          return INTERPRET_RUNTIME_ERROR;
        }

        push(fromNumber(-asNumber(pop())));
        break;
      }
      case OP_ADD: {
        BINARY_OP(fromNumber, +);
        break;
      }
      case OP_SUBTRACT: {
        BINARY_OP(fromNumber, -);
        break;
      }
      case OP_MULTIPLY: {
        BINARY_OP(fromNumber, *);
        break;
      }
      case OP_DIVIDE: {
        BINARY_OP(fromNumber, /);
        break;
      }
      case OP_EQUAL: {
        auto a = pop();
        auto b = pop();
        push(fromBool(valuesEqual(a, b)));
        break;
      }
      case OP_LESS: {
        BINARY_OP(fromBool, <);
        break;
      }
      case OP_GREATER: {
        BINARY_OP(fromBool, >);
        break;
      }
      case OP_RETURN: {
        printValue(pop());
        printf("\n");
        return INTERPRET_OK;
      }
    }
  }
#undef BINARY_OP
}

InterpreterResult interpret(const char* source) {
  Chunk chunk;
  initChunk(&chunk);

  if (!compile(source, &chunk)) {
    freeChunk(&chunk);
    return INTERPRET_COMPILE_ERROR;
  }

  vm.chunk = &chunk;
  vm.ip = chunk.code;

  auto result = run();

  freeChunk(&chunk);
  return result;
}
