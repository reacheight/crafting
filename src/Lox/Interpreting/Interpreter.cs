using Lox.Parsing;

namespace Lox.Interpreting;

public class Interpreter
{
    private record struct Unit;

    private readonly Environment environment = new();

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
        VarStmt varStmt => ExecuteVarStmt(varStmt),
        Block block => ExecuteBlock(block.Statements),
        IfStmt ifStmt => ExecuteIf(ifStmt),
    };

    private Unit ExecuteIf(IfStmt stmt)
    {
        var conditionResult = Evaluate(stmt.Condition);
        if (EvaluateAsBool(conditionResult))
            return Execute(stmt.OnTrue);

        if (stmt.OnFalse.HasValue)
            return Execute(stmt.OnFalse.Value);

        return new();
    }

    private Unit ExecuteBlock(List<Stmt> statements)
    {
        environment.EnterScope();

        foreach (var stmt in statements)
            Execute(stmt);

        environment.ExitScope();
        return new();
    }

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

    private Unit ExecuteVarStmt(VarStmt stmt)
    {
        var val = stmt.Initializer.HasValue
            ? Evaluate(stmt.Initializer.Value)
            : new Nil();

        environment.DefineVariable(stmt.Identifier.Lexeme, val);
        return new();
    }

    private LoxValue Evaluate(Expr expr) => expr switch
    {
        Literal literal => literal.Value,
        Variable variable => environment.GetVariableValue(variable.Identifier),
        Grouping grouping => Evaluate(grouping.Expr),
        UnaryExpr unary => EvaluateUnary(unary),
        BinaryExpr binary => EvaluateBinary(binary),
        Ternary ternary => EvaluateTernary(ternary),
        AssignmentExpr assignment => EvaluateAssignment(assignment),
    };

    private LoxValue EvaluateAssignment(AssignmentExpr assignment)
    {
        var val = Evaluate(assignment.Value);
        environment.AssignVariable(assignment.Target, val);
        return val;
    }

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
        return binary.Operator.Type switch
        {
            Add => (Evaluate(binary.Left), Evaluate(binary.Right)) switch
            {
                (double leftNumber, double rightNumber) => leftNumber + rightNumber,
                (string left, var right) => $"{left}{right}",
                (var left, string right) => $"{left}{right}",
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

            Equal => Evaluate(binary.Left) == Evaluate(binary.Right),
            NotEqual => Evaluate(binary.Left) != Evaluate(binary.Right),

            LogicalAnd => EvaluateAsBool(Evaluate(binary.Left)) && EvaluateAsBool(Evaluate(binary.Right)),
            LogicalOr => EvaluateAsBool(Evaluate(binary.Left)) || EvaluateAsBool(Evaluate(binary.Right)),
        };

        LoxValue ExecuteOnNumbers(Func<double, double, LoxValue> operation)
        {
            if (Evaluate(binary.Left) is double leftNum && Evaluate(binary.Right) is double rightNum)
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
