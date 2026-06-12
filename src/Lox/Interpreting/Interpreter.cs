using Lox.Parsing;

namespace Lox.Interpreting;

public class Interpreter
{
    private record struct Unit;

    public void Interpret(LoxProgram program)
    {
        try
        {
            foreach (var stmt in program.Statements)
                Execute(stmt);
        }
        catch (RuntimeException error)
        {
            Runner.ReportRuntimeError(error);
        }
    }

    private Unit Execute(Stmt stmt) => stmt switch
    {
        ExprStmt exprStmt => ExecuteExprStmt(exprStmt),
        PrintStmt printStmt => ExecutePrintStmt(printStmt),
    };

    private Unit ExecuteExprStmt(ExprStmt stmt)
    {
        Evaluate(stmt.Expr);
        return new();
    }

    private Unit ExecutePrintStmt(PrintStmt stmt)
    {
        var val = Evaluate(stmt.Expr);
        Console.WriteLine(val);
        return new();
    }

    private LoxValue Evaluate(Expr expr) => expr switch
    {
        Literal literal => literal.Value,
        Grouping grouping => Evaluate(grouping.Expr),
        UnaryExpr unary => EvaluateUnary(unary),
        BinaryExpr binary => EvaluateBinary(binary),
        Ternary ternary => EvaluateTernary(ternary),
    };

    private LoxValue EvaluateUnary(UnaryExpr unary)
    {
        var operandValue = Evaluate(unary.Expr);
        return unary.Operator.Type switch
        {
            Not => !EvaluateAsBool(operandValue),
            Negate => operandValue is double num
                ? -num
                : throw new RuntimeException(unary.Operator.Token, "Operand must be a number."),
        };
    }

    private LoxValue EvaluateBinary(BinaryExpr binary)
    {
        var leftValue = Evaluate(binary.Left);
        var rightValue = Evaluate(binary.Right);

        return binary.Operator.Type switch
        {
            Add => (leftValue, rightValue) switch
            {
                (double leftNumber, double rightNumber) => leftNumber + rightNumber,
                (string, _) or (_, string) => $"{leftValue}{rightValue}",
                _ => throw new RuntimeException(binary.Operator.Token, "Operands must be two numbers or at least on of them must be a string."),
            },
            Substract => ExecuteOnNumbers((l, r) => l - r),
            Divide => ExecuteOnNumbers((l, r) => r == 0
                ? throw new RuntimeException(binary.Operator.Token, "Division by zero.")
                : l / r),
            Multiply => ExecuteOnNumbers((l, r) => l * r),
            Greater => ExecuteOnNumbers((l, r) => l > r),
            GreaterEqual => ExecuteOnNumbers((l, r) => l >= r),
            Less => ExecuteOnNumbers((l, r) => l < r),
            LessEqual => ExecuteOnNumbers((l, r) => l <= r),

            Equal => leftValue.Value.Equals(rightValue.Value),
            NotEqual => !leftValue.Value.Equals(rightValue.Value),
        };

        LoxValue ExecuteOnNumbers(Func<double, double, LoxValue> operation)
        {
            if (leftValue is double leftNum && rightValue is double rightNum)
                return operation(leftNum, rightNum);

            throw new RuntimeException(binary.Operator.Token, "Operands must be two numbers.");
        }
    }

    private LoxValue EvaluateTernary(Ternary ternary)
        => EvaluateAsBool(Evaluate(ternary.Condition))
            ? Evaluate(ternary.OnTrue)
            : Evaluate(ternary.OnFalse);

    private static bool EvaluateAsBool(LoxValue literal) => literal switch
    {
        Nil => false,
        bool b => b,
        _ => true,
    };
}
