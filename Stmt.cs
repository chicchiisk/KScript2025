namespace Calculator;

public abstract class Stmt
{
    public abstract T Accept<T>(IStmtVisitor<T> visitor);
}

public interface IStmtVisitor<T>
{
    T VisitBlockStmt(BlockStmt stmt);
    T VisitExpressionStmt(ExpressionStmt stmt);
    T VisitForStmt(ForStmt stmt);
    T VisitFunctionStmt(FunctionStmt stmt);
    T VisitIfStmt(IfStmt stmt);
    T VisitReturnStmt(ReturnStmt stmt);
    T VisitVarStmt(VarStmt stmt);
}

public class BlockStmt : Stmt
{
    public List<Stmt> Statements { get; }

    public BlockStmt(List<Stmt> statements)
    {
        Statements = statements;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitBlockStmt(this);
    }
}

public class ForStmt : Stmt
{
    public Stmt? Initializer { get; }
    public Expr? Condition { get; }
    public Expr? Increment { get; }
    public Stmt Body { get; }

    public ForStmt(Stmt? initializer, Expr? condition, Expr? increment, Stmt body)
    {
        Initializer = initializer;
        Condition = condition;
        Increment = increment;
        Body = body;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitForStmt(this);
    }
}

public class IfStmt : Stmt
{
    public Expr Condition { get; }
    public Stmt ThenBranch { get; }
    public Stmt? ElseBranch { get; }

    public IfStmt(Expr condition, Stmt thenBranch, Stmt? elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitIfStmt(this);
    }
}

public class ExpressionStmt : Stmt
{
    public Expr Expression { get; }

    public ExpressionStmt(Expr expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitExpressionStmt(this);
    }
}

public class VarStmt : Stmt
{
    public Token Type { get; }
    public Token Name { get; }
    public int ArrayDimensions { get; }
    public Expr? Initializer { get; }

    public VarStmt(Token type, Token name, int arrayDimensions, Expr? initializer)
    {
        Type = type;
        Name = name;
        ArrayDimensions = arrayDimensions;
        Initializer = initializer;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitVarStmt(this);
    }
}

public class Parameter
{
    public Token Type { get; }
    public Token Name { get; }
    public int ArrayDimensions { get; }

    public Parameter(Token type, Token name, int arrayDimensions)
    {
        Type = type;
        Name = name;
        ArrayDimensions = arrayDimensions;
    }
}

public class FunctionStmt : Stmt
{
    public Token ReturnType { get; }
    public Token Name { get; }
    public int ReturnArrayDimensions { get; }
    public List<Parameter> Parameters { get; }
    public List<Stmt> Body { get; }

    public FunctionStmt(Token returnType, Token name, int returnArrayDimensions, List<Parameter> parameters, List<Stmt> body)
    {
        ReturnType = returnType;
        Name = name;
        ReturnArrayDimensions = returnArrayDimensions;
        Parameters = parameters;
        Body = body;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitFunctionStmt(this);
    }
}

public class ReturnStmt : Stmt
{
    public Expr? Value { get; }

    public ReturnStmt(Expr? value)
    {
        Value = value;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitReturnStmt(this);
    }
}