namespace Calculator;

#region Core Expression System

/// <summary>
/// Base class for all expressions in the AST
/// </summary>
public abstract class Expr
{
    public abstract T Accept<T>(IExprVisitor<T> visitor);
}

/// <summary>
/// Visitor interface for traversing expression nodes
/// </summary>
public interface IExprVisitor<T>
{
    // Basic expressions
    T VisitLiteralExpr(LiteralExpr expr);
    T VisitVariableExpr(VariableExpr expr);
    T VisitGroupingExpr(GroupingExpr expr);
    
    // Binary and unary operations
    T VisitBinaryExpr(BinaryExpr expr);
    T VisitUnaryExpr(UnaryExpr expr);
    T VisitPostfixExpr(PostfixExpr expr);
    
    // Assignment operations
    T VisitAssignExpr(AssignExpr expr);
    T VisitMemberAssignExpr(MemberAssignExpr expr);
    T VisitArrayAssignExpr(ArrayAssignExpr expr);
    
    // Function and object operations
    T VisitCallExpr(CallExpr expr);
    T VisitMemberAccessExpr(MemberAccessExpr expr);
    T VisitStructNewExpr(StructNewExpr expr);
    T VisitClassNewExpr(ClassNewExpr expr);
    
    // Array operations
    T VisitArrayAccessExpr(ArrayAccessExpr expr);
    T VisitArrayLiteralExpr(ArrayLiteralExpr expr);
    T VisitArrayNewExpr(ArrayNewExpr expr);
}

#endregion

#region Basic Expressions

/// <summary>
/// Represents a literal value (number, string, boolean, etc.)
/// </summary>
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

/// <summary>
/// Represents a variable reference
/// </summary>
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

/// <summary>
/// Represents a parenthesized expression
/// </summary>
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

#endregion

#region Arithmetic and Logical Operations

/// <summary>
/// Represents a binary operation (arithmetic, comparison, logical)
/// </summary>
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

/// <summary>
/// Represents a unary operation (prefix -, +, ++, --)
/// </summary>
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

/// <summary>
/// Represents a postfix operation (postfix ++, --)
/// </summary>
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

#endregion

#region Assignment Operations

/// <summary>
/// Represents a variable assignment
/// </summary>
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

/// <summary>
/// Represents a struct member assignment (obj.field = value)
/// </summary>
public class MemberAssignExpr : Expr
{
    public MemberAccessExpr Target { get; }
    public Expr Value { get; }

    public MemberAssignExpr(MemberAccessExpr target, Expr value)
    {
        Target = target;
        Value = value;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitMemberAssignExpr(this);
    }
}

/// <summary>
/// Represents an array element assignment (arr[i] = value)
/// </summary>
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

#endregion

#region Function and Object Operations

/// <summary>
/// Represents a function call
/// </summary>
public class CallExpr : Expr
{
    public Expr Callee { get; }
    public List<Expr> Arguments { get; }

    public CallExpr(Expr callee, List<Expr> arguments)
    {
        Callee = callee;
        Arguments = arguments;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitCallExpr(this);
    }
}

/// <summary>
/// Represents member access (obj.field or module.member)
/// </summary>
public class MemberAccessExpr : Expr
{
    public Token? ModuleName { get; }
    public Expr? Target { get; }
    public Token MemberName { get; }

    /// <summary>
    /// Constructor for module.member access
    /// </summary>
    public MemberAccessExpr(Token moduleName, Token memberName)
    {
        ModuleName = moduleName;
        Target = null;
        MemberName = memberName;
    }

    /// <summary>
    /// Constructor for expression.member access (e.g., array[0].field)
    /// </summary>
    public MemberAccessExpr(Expr target, Token memberName)
    {
        ModuleName = null;
        Target = target;
        MemberName = memberName;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitMemberAccessExpr(this);
    }
}

/// <summary>
/// Represents struct instantiation (new StructType())
/// </summary>
public class StructNewExpr : Expr
{
    public Token StructType { get; }

    public StructNewExpr(Token structType)
    {
        StructType = structType;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitStructNewExpr(this);
    }
}

/// <summary>
/// Represents class instantiation (new ClassName())
/// </summary>
public class ClassNewExpr : Expr
{
    public Token ClassType { get; }

    public ClassNewExpr(Token classType)
    {
        ClassType = classType;
    }

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitClassNewExpr(this);
    }
}

#endregion

#region Array Operations

/// <summary>
/// Represents array element access (arr[i] or arr[i,j])
/// </summary>
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

/// <summary>
/// Represents array literal creation ({1, 2, 3} or {{1, 2}, {3, 4}})
/// </summary>
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

/// <summary>
/// Represents array creation with dimensions (new int[5] or new int[3,4])
/// </summary>
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

#endregion