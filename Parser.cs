namespace Calculator;

public class Parser
{
    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        List<Stmt> statements = new();
        
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }
        
        return statements;
    }

    public Expr ParseExpression()
    {
        try
        {
            return Expression();
        }
        catch (Exception ex)
        {
            throw new Exception($"Parse error: {ex.Message}");
        }
    }

    private Stmt Declaration()
    {
        try
        {
            if (Check(TokenType.Int) || Check(TokenType.Float) || Check(TokenType.Char) || Check(TokenType.Bool) || Check(TokenType.Void))
            {
                return ParseTypeDeclaration();
            }
            return Statement();
        }
        catch (Exception ex)
        {
            throw new Exception($"Parse error: {ex.Message}");
        }
    }

    private Stmt ParseTypeDeclaration()
    {
        Token type = Advance(); // Consume the type token
        
        // Check for array dimensions
        int arrayDimensions = 0;
        while (Match(TokenType.LeftBracket))
        {
            if (arrayDimensions == 0)
            {
                arrayDimensions = 1;
                while (Match(TokenType.Comma))
                {
                    arrayDimensions++;
                }
            }
            else
            {
                while (Match(TokenType.Comma)) { }
            }
            Consume(TokenType.RightBracket, "Expect ']' after array type.");
        }
        
        Token name = Consume(TokenType.Identifier, "Expect identifier after type.");
        
        // Check if this is a function declaration (has parentheses)
        if (Check(TokenType.LeftParen))
        {
            return FunctionDeclaration(type, name, arrayDimensions);
        }
        else
        {
            return VariableDeclaration(type, name, arrayDimensions);
        }
    }

    private Stmt VariableDeclaration(Token type, Token name, int arrayDimensions)
    {
        Expr? initializer = null;
        if (Match(TokenType.Equal))
        {
            initializer = Expression();
        }

        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new VarStmt(type, name, arrayDimensions, initializer);
    }

    private Stmt FunctionDeclaration(Token returnType, Token name, int returnArrayDimensions)
    {
        Consume(TokenType.LeftParen, "Expect '(' after function name.");
        
        List<Parameter> parameters = new();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (!Check(TokenType.Int) && !Check(TokenType.Float) && !Check(TokenType.Char) && !Check(TokenType.Bool))
                {
                    throw new Exception($"Expect parameter type at position {Peek().Position}");
                }
                
                Token paramType = Advance();
                
                // Parse parameter array dimensions
                int paramArrayDimensions = 0;
                while (Match(TokenType.LeftBracket))
                {
                    if (paramArrayDimensions == 0)
                    {
                        paramArrayDimensions = 1;
                        while (Match(TokenType.Comma))
                        {
                            paramArrayDimensions++;
                        }
                    }
                    else
                    {
                        while (Match(TokenType.Comma)) { }
                    }
                    Consume(TokenType.RightBracket, "Expect ']' after parameter array type.");
                }
                
                Token paramName = Consume(TokenType.Identifier, "Expect parameter name.");
                parameters.Add(new Parameter(paramType, paramName, paramArrayDimensions));
                
            } while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.RightParen, "Expect ')' after parameters.");
        Consume(TokenType.LeftBrace, "Expect '{' before function body.");
        
        List<Stmt> body = new();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            body.Add(Declaration());
        }
        
        Consume(TokenType.RightBrace, "Expect '}' after function body.");
        
        return new FunctionStmt(returnType, name, returnArrayDimensions, parameters, body);
    }


    private Stmt Statement()
    {
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.LeftBrace)) return BlockStatement();
        
        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        Expr? value = null;
        if (!Check(TokenType.Semicolon))
        {
            value = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after return value.");
        return new ReturnStmt(value);
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (Match(TokenType.Semicolon))
        {
            initializer = null;
        }
        else if (Check(TokenType.Int) || Check(TokenType.Float) || Check(TokenType.Char) || Check(TokenType.Bool))
        {
            initializer = ParseTypeDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(TokenType.Semicolon))
        {
            condition = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = Expression();
        }
        Consume(TokenType.RightParen, "Expect ')' after for clauses.");

        Stmt body = Statement();

        return new ForStmt(initializer, condition, increment, body);
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(TokenType.Else))
        {
            elseBranch = Statement();
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private Stmt BlockStatement()
    {
        List<Stmt> statements = new();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return new BlockStmt(statements);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new ExpressionStmt(expr);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.Equal))
        {
            Token @operator = Previous();
            Expr value = Assignment();

            if (expr is VariableExpr variable)
            {
                Token name = variable.Name;
                return new AssignExpr(name, value);
            }
            else if (expr is ArrayAccessExpr arrayAccess)
            {
                return new ArrayAssignExpr(arrayAccess, value);
            }

            throw new Exception($"Invalid assignment target at position {@operator.Position}");
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.OrOr))
        {
            Token @operator = Previous();
            Expr right = And();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();

        while (Match(TokenType.AndAnd))
        {
            Token @operator = Previous();
            Expr right = Equality();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            Token @operator = Previous();
            Expr right = Comparison();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token @operator = Previous();
            Expr right = Term();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(TokenType.Plus, TokenType.Minus))
        {
            Token @operator = Previous();
            Expr right = Factor();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(TokenType.Multiply, TokenType.Divide, TokenType.Modulo))
        {
            Token @operator = Previous();
            Expr right = Unary();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.Minus, TokenType.Plus))
        {
            Token @operator = Previous();
            Expr right = Unary();
            return new UnaryExpr(@operator, right);
        }

        if (Match(TokenType.PlusPlus, TokenType.MinusMinus))
        {
            Token @operator = Previous();
            Expr right = Postfix(); // Change from Unary() to Postfix() to allow array access
            
            if (right is VariableExpr or ArrayAccessExpr)
            {
                return new UnaryExpr(@operator, right);
            }
            else
            {
                throw new Exception($"Invalid target for prefix {(@operator.Type == TokenType.PlusPlus ? "++" : "--")} operator at position {@operator.Position}");
            }
        }

        return Postfix();
    }

    private Expr Postfix()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                // Function call
                if (expr is VariableExpr variable)
                {
                    List<Expr> arguments = new();
                    if (!Check(TokenType.RightParen))
                    {
                        do
                        {
                            arguments.Add(Expression());
                        } while (Match(TokenType.Comma));
                    }
                    Consume(TokenType.RightParen, "Expect ')' after arguments.");
                    expr = new CallExpr(variable.Name, arguments);
                }
                else
                {
                    throw new Exception("Invalid function call target");
                }
            }
            else if (Match(TokenType.LeftBracket))
            {
                List<Expr> indices = new();
                indices.Add(Expression());
                
                while (Match(TokenType.Comma))
                {
                    indices.Add(Expression());
                }
                
                Consume(TokenType.RightBracket, "Expect ']' after array index.");
                expr = new ArrayAccessExpr(expr, indices);
            }
            else if (Match(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                Token @operator = Previous();
                
                if (expr is VariableExpr or ArrayAccessExpr)
                {
                    return new PostfixExpr(expr, @operator);
                }
                else
                {
                    throw new Exception($"Invalid target for {(@operator.Type == TokenType.PlusPlus ? "++" : "--")} operator at position {@operator.Position}");
                }
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(TokenType.True))
        {
            return new LiteralExpr(true);
        }

        if (Match(TokenType.False))
        {
            return new LiteralExpr(false);
        }

        if (Match(TokenType.Number))
        {
            return new LiteralExpr(Previous().Literal);
        }

        if (Match(TokenType.New))
        {
            if (!Check(TokenType.Int) && !Check(TokenType.Float) && !Check(TokenType.Char) && !Check(TokenType.Bool))
            {
                throw new Exception($"Expect type after 'new' at position {Peek().Position}");
            }
            
            Token type = Advance();
            Consume(TokenType.LeftBracket, "Expect '[' after type in new array expression.");
            
            List<Expr> dimensions = new();
            dimensions.Add(Expression());
            
            while (Match(TokenType.Comma))
            {
                dimensions.Add(Expression());
            }
            
            Consume(TokenType.RightBracket, "Expect ']' after array dimensions.");
            return new ArrayNewExpr(type, dimensions);
        }

        if (Match(TokenType.LeftBrace))
        {
            List<Expr> elements = new();
            
            if (!Check(TokenType.RightBrace))
            {
                do
                {
                    if (Check(TokenType.LeftBrace))
                    {
                        // Nested array literal for multi-dimensional arrays
                        elements.Add(Primary());
                    }
                    else
                    {
                        elements.Add(Expression());
                    }
                } while (Match(TokenType.Comma));
            }
            
            Consume(TokenType.RightBrace, "Expect '}' after array literal.");
            return new ArrayLiteralExpr(elements);
        }

        if (Match(TokenType.Identifier))
        {
            return new VariableExpr(Previous());
        }

        if (Match(TokenType.LeftParen))
        {
            Expr expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new GroupingExpr(expr);
        }

        throw new Exception($"Unexpected token '{Peek().Lexeme}' at position {Peek().Position}");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw new Exception($"{message} Got '{Peek().Lexeme}' at position {Peek().Position}");
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }
}