using Lox.Parsing;

namespace Lox.Interpreting;

public class Interpreter
{
    public void Interpret(Expr expr)
    {
        try
        {
            var value = EvaluateExpr(expr);
            Console.WriteLine(value);
        }
        catch (RuntimeException error)
        {
            Runner.ReportRuntimeError(error);
        }
    }
    private LoxValue EvaluateExpr(Expr expr) => expr switch
    {
        Literal literal => literal.Value,
        Grouping grouping => EvaluateExpr(grouping.Expr),
        UnaryExpr unary => EvaluateUnary(unary),
        BinaryExpr binary => EvaluateBinary(binary),
        Ternary ternary => EvaluateTernary(ternary),
    };

    private LoxValue EvaluateUnary(UnaryExpr unary)
    {
        var operandValue = EvaluateExpr(unary.Expr);
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
        var leftValue = EvaluateExpr(binary.Left);
        var rightValue = EvaluateExpr(binary.Right);

        return binary.Operator.Type switch
        {
            Add => (leftValue, rightValue) switch
            {
                (double leftNumber, double rightNumber) => leftNumber + rightNumber,
                (string leftString, string rightString) => leftString + rightString,
                _ => throw new RuntimeException(binary.Operator.Token, "Operands must be two numbers or two strings."),
            },
            Substract => ExecuteOnNumbers((l, r) => l - r),
            Divide => ExecuteOnNumbers((l, r) => l / r),
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
        => EvaluateAsBool(EvaluateExpr(ternary.Condition))
            ? EvaluateExpr(ternary.OnTrue)
            : EvaluateExpr(ternary.OnFalse);

    private static bool EvaluateAsBool(LoxValue literal) => literal switch
    {
        Nil => false,
        bool b => b,
        _ => true,
    };
}
