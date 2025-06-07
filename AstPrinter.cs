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

    public string VisitMemberAccessExpr(MemberAccessExpr expr)
    {
        if (expr.ModuleName != null)
        {
            return $"{expr.ModuleName.Lexeme}.{expr.MemberName.Lexeme}";
        }
        else if (expr.Target != null)
        {
            return $"({expr.Target.Accept(this)}).{expr.MemberName.Lexeme}";
        }
        else
        {
            return $"invalid.{expr.MemberName.Lexeme}";
        }
    }

    public string VisitMemberAssignExpr(MemberAssignExpr expr)
    {
        return $"{expr.Target.Accept(this)} = {expr.Value.Accept(this)}";
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
        string calleeName = expr.Callee switch
        {
            VariableExpr varExpr => varExpr.Name.Lexeme,
            MemberAccessExpr memberExpr => memberExpr.ModuleName?.Lexeme != null 
                ? $"{memberExpr.ModuleName.Lexeme}.{memberExpr.MemberName.Lexeme}"
                : $"({memberExpr.Target?.Accept(this)}).{memberExpr.MemberName.Lexeme}",
            _ => "call"
        };
        return Parenthesize($"call {calleeName}", expr.Arguments.ToArray());
    }

    public string VisitStructNewExpr(StructNewExpr expr)
    {
        return $"new {expr.StructType.Lexeme}()";
    }

    public string VisitClassNewExpr(ClassNewExpr expr)
    {
        return $"new {expr.ClassType.Lexeme}()";
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