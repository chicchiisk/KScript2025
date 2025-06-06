namespace Calculator;

public abstract class Expr
{
    public abstract T Accept<T>(IExprVisitor<T> visitor);
}

public interface IExprVisitor<T>
{
    T VisitArrayAccessExpr(ArrayAccessExpr expr);
    T VisitArrayAssignExpr(ArrayAssignExpr expr);
    T VisitArrayLiteralExpr(ArrayLiteralExpr expr);
    T VisitArrayNewExpr(ArrayNewExpr expr);
    T VisitAssignExpr(AssignExpr expr);
    T VisitBinaryExpr(BinaryExpr expr);
    T VisitCallExpr(CallExpr expr);
    T VisitGroupingExpr(GroupingExpr expr);
    T VisitLiteralExpr(LiteralExpr expr);
    T VisitPostfixExpr(PostfixExpr expr);
    T VisitUnaryExpr(UnaryExpr expr);
    T VisitVariableExpr(VariableExpr expr);
}

public class AssignExpr : Expr
{
    public Token Name { get; }
    public Expr Value { get; }

    public AssignExpr(Token name, Expr value)
    {
        Name = name;
        Value = value;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitAssignExpr(this);
    }
}

public class BinaryExpr : Expr
{
    public Expr Left { get; }
    public Token Operator { get; }
    public Expr Right { get; }

    public BinaryExpr(Expr left, Token @operator, Expr right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitBinaryExpr(this);
    }
}

public class CallExpr : Expr
{
    public Token Name { get; }
    public List<Expr> Arguments { get; }

    public CallExpr(Token name, List<Expr> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitCallExpr(this);
    }
}

public class GroupingExpr : Expr
{
    public Expr Expression { get; }

    public GroupingExpr(Expr expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitGroupingExpr(this);
    }
}

public class LiteralExpr : Expr
{
    public object? Value { get; }

    public LiteralExpr(object? value)
    {
        Value = value;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitLiteralExpr(this);
    }
}

public class PostfixExpr : Expr
{
    public Expr Expression { get; }
    public Token Operator { get; }

    public PostfixExpr(Expr expression, Token @operator)
    {
        Expression = expression;
        Operator = @operator;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitPostfixExpr(this);
    }
}

public class UnaryExpr : Expr
{
    public Token Operator { get; }
    public Expr Right { get; }

    public UnaryExpr(Token @operator, Expr right)
    {
        Operator = @operator;
        Right = right;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitUnaryExpr(this);
    }
}

public class VariableExpr : Expr
{
    public Token Name { get; }

    public VariableExpr(Token name)
    {
        Name = name;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitVariableExpr(this);
    }
}

public class ArrayAccessExpr : Expr
{
    public Expr Array { get; }
    public List<Expr> Indices { get; }

    public ArrayAccessExpr(Expr array, List<Expr> indices)
    {
        Array = array;
        Indices = indices;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitArrayAccessExpr(this);
    }
}

public class ArrayAssignExpr : Expr
{
    public ArrayAccessExpr Target { get; }
    public Expr Value { get; }

    public ArrayAssignExpr(ArrayAccessExpr target, Expr value)
    {
        Target = target;
        Value = value;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitArrayAssignExpr(this);
    }
}

public class ArrayLiteralExpr : Expr
{
    public List<Expr> Elements { get; }

    public ArrayLiteralExpr(List<Expr> elements)
    {
        Elements = elements;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitArrayLiteralExpr(this);
    }
}

public class ArrayNewExpr : Expr
{
    public Token Type { get; }
    public List<Expr> Dimensions { get; }

    public ArrayNewExpr(Token type, List<Expr> dimensions)
    {
        Type = type;
        Dimensions = dimensions;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitArrayNewExpr(this);
    }
}