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
            Substract => (double)leftValue.Value - (double)rightValue.Value,
            Divide => (double)leftValue.Value / (double)rightValue.Value,
            Multiply => (double)leftValue.Value * (double)rightValue.Value,

            Greater => (double)leftValue.Value > (double)rightValue.Value,
            GreaterEqual => (double)leftValue.Value >= (double)rightValue.Value,
            Less => (double)leftValue.Value < (double)rightValue.Value,
            LessEqual => (double)leftValue.Value <= (double)rightValue.Value,

            Equal => leftValue.Value.Equals(rightValue.Value),
            NotEqual => !leftValue.Value.Equals(rightValue.Value),
        };
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
