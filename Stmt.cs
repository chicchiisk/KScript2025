namespace Calculator;

#region Core Statement System

/// <summary>
/// Base class for all statements in the AST
/// </summary>
public abstract class Stmt
{
    public abstract T Accept<T>(IStmtVisitor<T> visitor);
}

/// <summary>
/// Visitor interface for traversing statement nodes
/// </summary>
public interface IStmtVisitor<T>
{
    // Basic statements
    T VisitExpressionStmt(ExpressionStmt stmt);
    T VisitBlockStmt(BlockStmt stmt);
    
    // Control flow statements
    T VisitIfStmt(IfStmt stmt);
    T VisitForStmt(ForStmt stmt);
    T VisitWhileStmt(WhileStmt stmt);
    T VisitReturnStmt(ReturnStmt stmt);
    
    // Declaration statements
    T VisitVarStmt(VarStmt stmt);
    T VisitFunctionStmt(FunctionStmt stmt);
    T VisitStructStmt(StructStmt stmt);
    T VisitClassStmt(ClassStmt stmt);
    
    // Module system statements
    T VisitImportStmt(ImportStmt stmt);
}

#endregion

#region Basic Statements

/// <summary>
/// Represents an expression used as a statement
/// </summary>
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

/// <summary>
/// Represents a block of statements enclosed in braces
/// </summary>
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

#endregion

#region Control Flow Statements

/// <summary>
/// Represents an if-else conditional statement
/// </summary>
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

/// <summary>
/// Represents a for loop statement
/// </summary>
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

/// <summary>
/// Represents a while loop statement
/// </summary>
public class WhileStmt : Stmt
{
    public Expr Condition { get; }
    public Stmt Body { get; }

    public WhileStmt(Expr condition, Stmt body)
    {
        Condition = condition;
        Body = body;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitWhileStmt(this);
    }
}

/// <summary>
/// Represents a return statement
/// </summary>
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

#endregion

#region Declaration Statements

/// <summary>
/// Represents a variable declaration with optional initialization
/// </summary>
public class VarStmt : Stmt
{
    public bool IsExported { get; }
    public Token Type { get; }
    public Token Name { get; }
    public int ArrayDimensions { get; }
    public Expr? Initializer { get; }

    public VarStmt(bool isExported, Token type, Token name, int arrayDimensions, Expr? initializer)
    {
        IsExported = isExported;
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

/// <summary>
/// Represents a function parameter with type information
/// </summary>
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

/// <summary>
/// Represents a function declaration with parameters and body
/// </summary>
public class FunctionStmt : Stmt
{
    public bool IsExported { get; }
    public Token ReturnType { get; }
    public Token Name { get; }
    public int ReturnArrayDimensions { get; }
    public List<Parameter> Parameters { get; }
    public List<Stmt> Body { get; }

    public FunctionStmt(bool isExported, Token returnType, Token name, int returnArrayDimensions, List<Parameter> parameters, List<Stmt> body)
    {
        IsExported = isExported;
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

/// <summary>
/// Represents a struct declaration with fields, methods, and optional constructor
/// </summary>
public class StructStmt : Stmt
{
    public bool IsExported { get; }
    public Token Name { get; }
    public List<VarStmt> Fields { get; }
    public List<FunctionStmt> Methods { get; }
    public FunctionStmt? Constructor { get; }

    public StructStmt(bool isExported, Token name, List<VarStmt> fields, List<FunctionStmt> methods, FunctionStmt? constructor)
    {
        IsExported = isExported;
        Name = name;
        Fields = fields;
        Methods = methods;
        Constructor = constructor;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitStructStmt(this);
    }
}

/// <summary>
/// Represents a class declaration with fields, methods, and optional constructor
/// </summary>
public class ClassStmt : Stmt
{
    public bool IsExported { get; }
    public Token Name { get; }
    public List<VarStmt> Fields { get; }
    public List<FunctionStmt> Methods { get; }
    public FunctionStmt? Constructor { get; }

    public ClassStmt(bool isExported, Token name, List<VarStmt> fields, List<FunctionStmt> methods, FunctionStmt? constructor)
    {
        IsExported = isExported;
        Name = name;
        Fields = fields;
        Methods = methods;
        Constructor = constructor;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitClassStmt(this);
    }
}

#endregion

#region Module System Statements

/// <summary>
/// Represents an import statement for loading external modules
/// </summary>
public class ImportStmt : Stmt
{
    public string Path { get; }
    public string? ModuleName { get; }

    /// <summary>
    /// Creates an import statement
    /// </summary>
    /// <param name="path">The path to the module file</param>
    /// <param name="moduleName">Optional module name for named imports</param>
    public ImportStmt(string path, string? moduleName = null)
    {
        Path = path;
        ModuleName = moduleName;
    }

    public override T Accept<T>(IStmtVisitor<T> visitor)
    {
        return visitor.VisitImportStmt(this);
    }
}

#endregion