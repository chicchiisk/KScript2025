namespace Calculator;

public class AstPrinter : IExprVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitBinaryExpr(BinaryExpr expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(GroupingExpr expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(LiteralExpr expr)
    {
        return expr.Value?.ToString() ?? "nil";
    }

    public string VisitUnaryExpr(UnaryExpr expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    public string VisitVariableExpr(VariableExpr expr)
    {
        return expr.Name.Lexeme;
    }

    public string VisitAssignExpr(AssignExpr expr)
    {
        return Parenthesize("=", new VariableExpr(expr.Name), expr.Value);
    }

    public string VisitPostfixExpr(PostfixExpr expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Expression);
    }

    public string VisitArrayAccessExpr(ArrayAccessExpr expr)
    {
        return Parenthesize("[]", new Expr[] { expr.Array }.Concat(expr.Indices).ToArray());
    }

    public string VisitArrayAssignExpr(ArrayAssignExpr expr)
    {
        return Parenthesize("=[]", expr.Target, expr.Value);
    }

    public string VisitArrayLiteralExpr(ArrayLiteralExpr expr)
    {
        return Parenthesize("{}", expr.Elements.ToArray());
    }

    public string VisitArrayNewExpr(ArrayNewExpr expr)
    {
        return Parenthesize("new[]", expr.Dimensions.ToArray());
    }

    public string VisitCallExpr(CallExpr expr)
    {
        return Parenthesize($"call {expr.Name.Lexeme}", expr.Arguments.ToArray());
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var result = $"({name}";
        
        foreach (Expr expr in exprs)
        {
            result += " " + expr.Accept(this);
        }
        
        result += ")";
        return result;
    }
}