#include "vm.h"
#include "chunk.h"
#include "compiler.h"
#include "debug.h"
#include "value.h"
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>

VM vm;

static void resetStack() {
    vm.stackTop = vm.stack;
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
#define BINARY_OP(op)                                                                                                  \
    do {                                                                                                               \
        double b = pop();                                                                                              \
        double a = pop();                                                                                              \
        push(a op b);                                                                                                  \
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
            case OP_NEGATE: {
                push(-pop());
                break;
            }
            case OP_ADD: {
                BINARY_OP(+);
                break;
            }
            case OP_SUBTRACT: {
                BINARY_OP(-);
                break;
            }
            case OP_MULTIPLY: {
                BINARY_OP(*);
                break;
            }
            case OP_DIVIDE: {
                BINARY_OP(/);
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
    compile(source);
    return INTERPRET_OK;
}
