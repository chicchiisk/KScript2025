namespace Calculator;

/// <summary>
/// Recursive descent parser for the scripting language
/// </summary>
public class Parser
{
    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    #region Public Interface

    /// <summary>
    /// Parses a list of statements from the tokens
    /// </summary>
    public List<Stmt> Parse()
    {
        List<Stmt> statements = new();
        
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }
        
        return statements;
    }

    /// <summary>
    /// Parses a single expression from the tokens
    /// </summary>
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

    #endregion

    #region Declaration Parsing

    /// <summary>
    /// Parses declarations (imports, exports, functions, variables, structs)
    /// </summary>
    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.Import)) return ImportDeclaration();
            if (Match(TokenType.Export)) return ExportDeclaration();
            if (Match(TokenType.Struct)) return StructDeclaration();
            if (Match(TokenType.Class)) return ClassDeclaration();
            
            if (IsPrimitiveType(Peek()))
            {
                return ParseTypeDeclaration();
            }
            
            // Check for struct type declarations
            if (IsStructTypeDeclaration())
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

    /// <summary>
    /// Parses import declarations
    /// </summary>
    private Stmt ImportDeclaration()
    {
        // Check for 'import as module from "path";' syntax
        if (Check(TokenType.As))
        {
            Advance(); // consume 'as'
            Token moduleNameToken = Consume(TokenType.Identifier, "Expect module name after 'as'.");
            Consume(TokenType.From, "Expect 'from' after module name.");
            Token pathToken = Consume(TokenType.StringLiteral, "Expect string path after 'from'.");
            Consume(TokenType.Semicolon, "Expect ';' after import statement.");
            
            if (pathToken.Literal is not string path)
            {
                throw new Exception($"Import path must be a string literal at position {pathToken.Position}");
            }
            
            return new ImportStmt(path, moduleNameToken.Lexeme);
        }
        // Handle existing 'import "path";' syntax
        else
        {
            Token pathToken = Consume(TokenType.StringLiteral, "Expect string path after 'import'.");
            Consume(TokenType.Semicolon, "Expect ';' after import path.");
            
            if (pathToken.Literal is not string path)
            {
                throw new Exception($"Import path must be a string literal at position {pathToken.Position}");
            }
            
            return new ImportStmt(path);
        }
    }

    /// <summary>
    /// Parses export declarations
    /// </summary>
    private Stmt ExportDeclaration()
    {
        if (Match(TokenType.Struct))
        {
            return StructDeclaration(true);
        }
        else if (Match(TokenType.Class))
        {
            return ClassDeclaration(true);
        }
        else if (IsPrimitiveType(Peek()) || Check(TokenType.Identifier))
        {
            return ParseTypeDeclaration(true);
        }
        else
        {
            throw new Exception($"Only variables, functions, structs, and classes can be exported at position {Peek().Position}");
        }
    }

    /// <summary>
    /// Parses struct declarations
    /// </summary>
    private Stmt StructDeclaration(bool isExported = false)
    {
        Token name = Consume(TokenType.Identifier, "Expect struct name.");
        Consume(TokenType.LeftBrace, "Expect '{' before struct body.");
        
        List<VarStmt> fields = new();
        List<FunctionStmt> methods = new();
        FunctionStmt? constructor = null;
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            if (IsPrimitiveType(Peek()))
            {
                ParseStructMember(name, fields, methods, ref constructor);
            }
            else if (Check(TokenType.Identifier))
            {
                ParseStructIdentifierMember(name, fields, methods, ref constructor);
            }
            else
            {
                throw new Exception($"Unexpected token in struct body at position {Peek().Position}");
            }
        }
        
        Consume(TokenType.RightBrace, "Expect '}' after struct body.");
        return new StructStmt(isExported, name, fields, methods, constructor);
    }

    /// <summary>
    /// Parses class declarations
    /// </summary>
    private Stmt ClassDeclaration(bool isExported = false)
    {
        Token name = Consume(TokenType.Identifier, "Expect class name.");
        Consume(TokenType.LeftBrace, "Expect '{' before class body.");
        
        List<VarStmt> fields = new();
        List<FunctionStmt> methods = new();
        FunctionStmt? constructor = null;
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            if (IsPrimitiveType(Peek()))
            {
                ParseClassMember(name, fields, methods, ref constructor);
            }
            else if (Check(TokenType.Identifier))
            {
                ParseClassIdentifierMember(name, fields, methods, ref constructor);
            }
            else
            {
                throw new Exception($"Unexpected token in class body at position {Peek().Position}");
            }
        }
        
        Consume(TokenType.RightBrace, "Expect '}' after class body.");
        return new ClassStmt(isExported, name, fields, methods, constructor);
    }

    /// <summary>
    /// Parses type declarations (variables and functions)
    /// </summary>
    private Stmt ParseTypeDeclaration(bool isExported = false)
    {
        Token type = Advance();
        int arrayDimensions = ParseArrayDimensions();
        Token name = Consume(TokenType.Identifier, "Expect identifier after type.");
        
        return Check(TokenType.LeftParen) 
            ? FunctionDeclaration(isExported, type, name, arrayDimensions)
            : VariableDeclaration(isExported, type, name, arrayDimensions);
    }

    /// <summary>
    /// Parses variable declarations
    /// </summary>
    private Stmt VariableDeclaration(bool isExported, Token type, Token name, int arrayDimensions)
    {
        Expr? initializer = null;
        if (Match(TokenType.Equal))
        {
            initializer = Expression();
        }

        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new VarStmt(isExported, type, name, arrayDimensions, initializer);
    }

    /// <summary>
    /// Parses function declarations
    /// </summary>
    private Stmt FunctionDeclaration(bool isExported, Token returnType, Token name, int returnArrayDimensions)
    {
        Consume(TokenType.LeftParen, "Expect '(' after function name.");
        
        List<Parameter> parameters = ParseParameterList();
        
        Consume(TokenType.RightParen, "Expect ')' after parameters.");
        Consume(TokenType.LeftBrace, "Expect '{' before function body.");
        
        List<Stmt> body = ParseBlockStatements();
        
        Consume(TokenType.RightBrace, "Expect '}' after function body.");
        
        return new FunctionStmt(isExported, returnType, name, returnArrayDimensions, parameters, body);
    }

    #endregion

    #region Statement Parsing

    /// <summary>
    /// Parses statements
    /// </summary>
    private Stmt Statement()
    {
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.While)) return WhileStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.LeftBrace)) return BlockStatement();
        
        return ExpressionStatement();
    }

    /// <summary>
    /// Parses return statements
    /// </summary>
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

    /// <summary>
    /// Parses for loop statements
    /// </summary>
    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

        Stmt? initializer = ParseForInitializer();
        Expr? condition = ParseForCondition();
        Expr? increment = ParseForIncrement();

        Consume(TokenType.RightParen, "Expect ')' after for clauses.");
        Stmt body = Statement();

        return new ForStmt(initializer, condition, increment, body);
    }

    /// <summary>
    /// Parses while statements
    /// </summary>
    private Stmt WhileStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        Expr condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after while condition.");
        
        Stmt body = Statement();
        
        return new WhileStmt(condition, body);
    }

    /// <summary>
    /// Parses if statements
    /// </summary>
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

    /// <summary>
    /// Parses block statements
    /// </summary>
    private Stmt BlockStatement()
    {
        List<Stmt> statements = ParseBlockStatements();
        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return new BlockStmt(statements);
    }

    /// <summary>
    /// Parses expression statements
    /// </summary>
    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new ExpressionStmt(expr);
    }

    #endregion

    #region Expression Parsing

    /// <summary>
    /// Parses expressions
    /// </summary>
    private Expr Expression()
    {
        return Assignment();
    }

    /// <summary>
    /// Parses assignment expressions
    /// </summary>
    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.Equal))
        {
            Token @operator = Previous();
            Expr value = Assignment();

            return expr switch
            {
                VariableExpr variable => new AssignExpr(variable.Name, value),
                ArrayAccessExpr arrayAccess => new ArrayAssignExpr(arrayAccess, value),
                MemberAccessExpr memberAccess => new MemberAssignExpr(memberAccess, value),
                _ => throw new Exception($"Invalid assignment target at position {@operator.Position}")
            };
        }

        return expr;
    }

    /// <summary>
    /// Parses logical OR expressions
    /// </summary>
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

    /// <summary>
    /// Parses logical AND expressions
    /// </summary>
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

    /// <summary>
    /// Parses equality expressions
    /// </summary>
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

    /// <summary>
    /// Parses comparison expressions
    /// </summary>
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

    /// <summary>
    /// Parses addition and subtraction expressions
    /// </summary>
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

    /// <summary>
    /// Parses multiplication, division, and modulo expressions
    /// </summary>
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

    /// <summary>
    /// Parses unary expressions
    /// </summary>
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
            Expr right = Postfix();
            
            if (right is not (VariableExpr or ArrayAccessExpr))
            {
                throw new Exception($"Invalid target for prefix {(@operator.Type == TokenType.PlusPlus ? "++" : "--")} operator at position {@operator.Position}");
            }
            
            return new UnaryExpr(@operator, right);
        }

        return Postfix();
    }

    /// <summary>
    /// Parses postfix expressions (calls, array access, member access, postfix operators)
    /// </summary>
    private Expr Postfix()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = ParseFunctionCall(expr);
            }
            else if (Match(TokenType.LeftBracket))
            {
                expr = ParseArrayAccess(expr);
            }
            else if (Match(TokenType.Dot))
            {
                expr = ParseMemberAccess(expr);
            }
            else if (Match(TokenType.PlusPlus, TokenType.MinusMinus))
            {
                return ParsePostfixOperator(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    /// <summary>
    /// Parses primary expressions
    /// </summary>
    private Expr Primary()
    {
        if (Match(TokenType.True)) return new LiteralExpr(true);
        if (Match(TokenType.False)) return new LiteralExpr(false);
        if (Match(TokenType.Number)) return new LiteralExpr(Previous().Literal);
        if (Match(TokenType.StringLiteral)) return new LiteralExpr(Previous().Literal);

        if (Match(TokenType.New)) return ParseNewExpression();
        if (Match(TokenType.LeftBrace)) return ParseArrayLiteral();
        if (Match(TokenType.Identifier)) return ParseIdentifierExpression();
        if (Match(TokenType.LeftParen)) return ParseGroupingExpression();

        throw new Exception($"Unexpected token '{Peek().Lexeme}' at position {Peek().Position}");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if token represents a primitive type
    /// </summary>
    private static bool IsPrimitiveType(Token token)
    {
        return token.Type is TokenType.Int or TokenType.Float or TokenType.Char or TokenType.Bool or TokenType.Void or TokenType.String;
    }

    /// <summary>
    /// Checks if current position indicates a struct type declaration
    /// </summary>
    private bool IsStructTypeDeclaration()
    {
        // Check for "identifier identifier" pattern
        if (Check(TokenType.Identifier) && CheckNext(TokenType.Identifier))
        {
            return true;
        }
        
        // Check for "identifier[] identifier" pattern
        if (Check(TokenType.Identifier) && CheckNext(TokenType.LeftBracket))
        {
            int lookahead = current + 1;
            while (lookahead < tokens.Count && tokens[lookahead].Type == TokenType.LeftBracket)
            {
                lookahead++;
                while (lookahead < tokens.Count && tokens[lookahead].Type != TokenType.RightBracket)
                {
                    lookahead++;
                }
                if (lookahead < tokens.Count && tokens[lookahead].Type == TokenType.RightBracket)
                {
                    lookahead++;
                }
            }
            return lookahead < tokens.Count && tokens[lookahead].Type == TokenType.Identifier;
        }
        
        return false;
    }

    /// <summary>
    /// Parses array dimensions notation
    /// </summary>
    private int ParseArrayDimensions()
    {
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
        return arrayDimensions;
    }

    /// <summary>
    /// Parses parameter list for functions
    /// </summary>
    private List<Parameter> ParseParameterList()
    {
        List<Parameter> parameters = new();
        
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (!IsPrimitiveType(Peek()) && !Check(TokenType.Identifier))
                {
                    throw new Exception($"Expect parameter type at position {Peek().Position}");
                }
                
                Token paramType = Advance();
                int paramArrayDimensions = ParseArrayDimensions();
                Token paramName = Consume(TokenType.Identifier, "Expect parameter name.");
                parameters.Add(new Parameter(paramType, paramName, paramArrayDimensions));
                
            } while (Match(TokenType.Comma));
        }
        
        return parameters;
    }

    /// <summary>
    /// Parses statements inside a block
    /// </summary>
    private List<Stmt> ParseBlockStatements()
    {
        List<Stmt> statements = new();
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }
        
        return statements;
    }

    /// <summary>
    /// Parses for loop initializer
    /// </summary>
    private Stmt? ParseForInitializer()
    {
        if (Match(TokenType.Semicolon))
        {
            return null;
        }
        else if (IsPrimitiveType(Peek()))
        {
            return ParseTypeDeclaration();
        }
        else
        {
            return ExpressionStatement();
        }
    }

    /// <summary>
    /// Parses for loop condition
    /// </summary>
    private Expr? ParseForCondition()
    {
        Expr? condition = null;
        if (!Check(TokenType.Semicolon))
        {
            condition = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");
        return condition;
    }

    /// <summary>
    /// Parses for loop increment
    /// </summary>
    private Expr? ParseForIncrement()
    {
        Expr? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = Expression();
        }
        return increment;
    }

    /// <summary>
    /// Parses function call expressions
    /// </summary>
    private Expr ParseFunctionCall(Expr callee)
    {
        if (callee is not (VariableExpr or MemberAccessExpr))
        {
            throw new Exception("Invalid function call target");
        }

        List<Expr> arguments = new();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expect ')' after arguments.");
        return new CallExpr(callee, arguments);
    }

    /// <summary>
    /// Parses array access expressions
    /// </summary>
    private Expr ParseArrayAccess(Expr array)
    {
        List<Expr> indices = new() { Expression() };
        
        while (Match(TokenType.Comma))
        {
            indices.Add(Expression());
        }
        
        Consume(TokenType.RightBracket, "Expect ']' after array index.");
        return new ArrayAccessExpr(array, indices);
    }

    /// <summary>
    /// Parses member access expressions
    /// </summary>
    private Expr ParseMemberAccess(Expr target)
    {
        Token memberName = Consume(TokenType.Identifier, "Expect member name after '.'.");
        
        return target switch
        {
            VariableExpr varExpr => new MemberAccessExpr(varExpr.Name, memberName),
            _ => new MemberAccessExpr(target, memberName)
        };
    }

    /// <summary>
    /// Parses postfix operators
    /// </summary>
    private Expr ParsePostfixOperator(Expr target)
    {
        Token @operator = Previous();
        
        if (target is not (VariableExpr or ArrayAccessExpr))
        {
            throw new Exception($"Invalid target for {(@operator.Type == TokenType.PlusPlus ? "++" : "--")} operator at position {@operator.Position}");
        }
        
        return new PostfixExpr(target, @operator);
    }

    /// <summary>
    /// Parses new expressions (arrays and structs)
    /// </summary>
    private Expr ParseNewExpression()
    {
        if (!IsPrimitiveType(Peek()) && !Check(TokenType.Identifier))
        {
            throw new Exception($"Expect type or struct name after 'new' at position {Peek().Position}");
        }

        Token type = Advance();
        
        if (Check(TokenType.LeftBracket))
        {
            return ParseArrayNewExpression(type);
        }
        else if (Check(TokenType.LeftParen))
        {
            return ParseObjectNewExpression(type);
        }
        else
        {
            throw new Exception($"Expect '[' or '(' after type in new expression at position {Peek().Position}");
        }
    }

    /// <summary>
    /// Parses array creation expressions
    /// </summary>
    private Expr ParseArrayNewExpression(Token type)
    {
        Advance(); // consume '['
        
        List<Expr> dimensions = new() { Expression() };
        
        while (Match(TokenType.Comma))
        {
            dimensions.Add(Expression());
        }
        
        Consume(TokenType.RightBracket, "Expect ']' after array dimensions.");
        return new ArrayNewExpr(type, dimensions);
    }

    /// <summary>
    /// Parses struct instantiation expressions
    /// </summary>
    private Expr ParseObjectNewExpression(Token type)
    {
        Advance(); // consume '('
        Consume(TokenType.RightParen, "Expect ')' after constructor arguments (parameterless constructor only).");
        // For now, we'll use StructNewExpr for both structs and classes
        // The interpreter will determine the actual type during execution
        return new StructNewExpr(type);
    }

    private Expr ParseStructNewExpression(Token type)
    {
        Advance(); // consume '('
        Consume(TokenType.RightParen, "Expect ')' after constructor arguments (parameterless constructor only).");
        return new StructNewExpr(type);
    }

    /// <summary>
    /// Parses array literal expressions
    /// </summary>
    private Expr ParseArrayLiteral()
    {
        List<Expr> elements = new();
        
        if (!Check(TokenType.RightBrace))
        {
            do
            {
                elements.Add(Check(TokenType.LeftBrace) ? Primary() : Expression());
            } while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.RightBrace, "Expect '}' after array literal.");
        return new ArrayLiteralExpr(elements);
    }

    /// <summary>
    /// Parses identifier expressions
    /// </summary>
    private Expr ParseIdentifierExpression()
    {
        Token identifier = Previous();
        
        if (Match(TokenType.Dot))
        {
            Token memberName = Consume(TokenType.Identifier, "Expect member name after '.'.");
            return new MemberAccessExpr(identifier, memberName);
        }
        
        return new VariableExpr(identifier);
    }

    /// <summary>
    /// Parses grouping expressions
    /// </summary>
    private Expr ParseGroupingExpression()
    {
        Expr expr = Expression();
        Consume(TokenType.RightParen, "Expect ')' after expression.");
        return new GroupingExpr(expr);
    }

    /// <summary>
    /// Parses struct members (fields and methods)
    /// </summary>
    private void ParseStructMember(Token structName, List<VarStmt> fields, List<FunctionStmt> methods, ref FunctionStmt? constructor)
    {
        Token type = Advance();
        int arrayDimensions = ParseArrayDimensions();
        Token memberName = Consume(TokenType.Identifier, "Expect member name after type.");
        
        if (Check(TokenType.Semicolon))
        {
            // Field
            Advance();
            VarStmt field = new(false, type, memberName, arrayDimensions, null);
            fields.Add(field);
        }
        else if (Check(TokenType.LeftParen))
        {
            // Method
            Advance();
            List<Parameter> parameters = ParseParameterList();
            Consume(TokenType.RightParen, "Expect ')' after parameters.");
            Consume(TokenType.LeftBrace, "Expect '{' before method body.");
            List<Stmt> body = ParseBlockStatements();
            Consume(TokenType.RightBrace, "Expect '}' after method body.");
            
            FunctionStmt method = new(false, type, memberName, 0, parameters, body);
            
            // Check if this is a constructor
            if (memberName.Lexeme == structName.Lexeme && type.Type == TokenType.Void)
            {
                if (constructor != null)
                {
                    throw new Exception($"Struct '{structName.Lexeme}' already has a constructor at position {memberName.Position}");
                }
                constructor = method;
            }
            else
            {
                methods.Add(method);
            }
        }
        else
        {
            throw new Exception($"Expect ';' or '(' after member name at position {Peek().Position}");
        }
    }

    /// <summary>
    /// Parses struct members when starting with identifier
    /// </summary>
    private void ParseStructIdentifierMember(Token structName, List<VarStmt> fields, List<FunctionStmt> methods, ref FunctionStmt? constructor)
    {
        Token firstIdentifier = Advance();
        
        // Check if this is a constructor
        if (firstIdentifier.Lexeme == structName.Lexeme && Check(TokenType.LeftParen))
        {
            if (constructor != null)
            {
                throw new Exception($"Struct '{structName.Lexeme}' already has a constructor at position {firstIdentifier.Position}");
            }
            
            Advance(); // consume '('
            List<Parameter> parameters = ParseParameterList();
            Consume(TokenType.RightParen, "Expect ')' after parameters.");
            Consume(TokenType.LeftBrace, "Expect '{' before constructor body.");
            List<Stmt> body = ParseBlockStatements();
            Consume(TokenType.RightBrace, "Expect '}' after constructor body.");
            
            Token voidToken = new(TokenType.Void, "void", null, firstIdentifier.Position);
            constructor = new FunctionStmt(false, voidToken, firstIdentifier, 0, parameters, body);
        }
        else if (Check(TokenType.Identifier))
        {
            // Field or method with user-defined type
            Token memberName = Advance();
            
            if (Check(TokenType.LeftParen))
            {
                // Method with user-defined return type
                Advance();
                List<Parameter> parameters = ParseParameterList();
                Consume(TokenType.RightParen, "Expect ')' after parameters.");
                Consume(TokenType.LeftBrace, "Expect '{' before method body.");
                List<Stmt> body = ParseBlockStatements();
                Consume(TokenType.RightBrace, "Expect '}' after method body.");
                
                FunctionStmt method = new(false, firstIdentifier, memberName, 0, parameters, body);
                methods.Add(method);
            }
            else
            {
                // Field with user-defined type
                int arrayDimensions = ParseArrayDimensions();
                Consume(TokenType.Semicolon, "Expect ';' after field declaration.");
                VarStmt field = new(false, firstIdentifier, memberName, arrayDimensions, null);
                fields.Add(field);
            }
        }
        else
        {
            throw new Exception($"Unexpected identifier '{firstIdentifier.Lexeme}' in struct body at position {firstIdentifier.Position}");
        }
    }

    /// <summary>
    /// Parses class members starting with primitive types
    /// </summary>
    private void ParseClassMember(Token className, List<VarStmt> fields, List<FunctionStmt> methods, ref FunctionStmt? constructor)
    {
        Token type = Advance();
        int arrayDimensions = ParseArrayDimensions();
        Token memberName = Consume(TokenType.Identifier, "Expect member name after type.");
        
        if (Check(TokenType.Semicolon))
        {
            // Field
            Advance();
            VarStmt field = new(false, type, memberName, arrayDimensions, null);
            fields.Add(field);
        }
        else if (Check(TokenType.LeftParen))
        {
            // Method
            Advance();
            List<Parameter> parameters = ParseParameterList();
            Consume(TokenType.RightParen, "Expect ')' after parameters.");
            Consume(TokenType.LeftBrace, "Expect '{' before method body.");
            List<Stmt> body = ParseBlockStatements();
            Consume(TokenType.RightBrace, "Expect '}' after method body.");
            
            FunctionStmt method = new(false, type, memberName, 0, parameters, body);
            
            // Check if this is a constructor
            if (memberName.Lexeme == className.Lexeme && type.Type == TokenType.Void)
            {
                if (constructor != null)
                {
                    throw new Exception($"Class '{className.Lexeme}' already has a constructor at position {memberName.Position}");
                }
                constructor = method;
            }
            else
            {
                methods.Add(method);
            }
        }
        else
        {
            throw new Exception($"Expect ';' or '(' after member name at position {Peek().Position}");
        }
    }

    /// <summary>
    /// Parses class members when starting with identifier
    /// </summary>
    private void ParseClassIdentifierMember(Token className, List<VarStmt> fields, List<FunctionStmt> methods, ref FunctionStmt? constructor)
    {
        Token firstIdentifier = Advance();
        
        // Check if this is a constructor
        if (firstIdentifier.Lexeme == className.Lexeme && Check(TokenType.LeftParen))
        {
            if (constructor != null)
            {
                throw new Exception($"Class '{className.Lexeme}' already has a constructor at position {firstIdentifier.Position}");
            }
            
            Advance(); // consume '('
            List<Parameter> parameters = ParseParameterList();
            Consume(TokenType.RightParen, "Expect ')' after parameters.");
            Consume(TokenType.LeftBrace, "Expect '{' before constructor body.");
            List<Stmt> body = ParseBlockStatements();
            Consume(TokenType.RightBrace, "Expect '}' after constructor body.");
            
            Token voidToken = new(TokenType.Void, "void", null, firstIdentifier.Position);
            constructor = new FunctionStmt(false, voidToken, firstIdentifier, 0, parameters, body);
        }
        else if (Check(TokenType.Identifier))
        {
            // Field or method with user-defined type
            Token memberName = Advance();
            
            if (Check(TokenType.LeftParen))
            {
                // Method with user-defined return type
                Advance();
                List<Parameter> parameters = ParseParameterList();
                Consume(TokenType.RightParen, "Expect ')' after parameters.");
                Consume(TokenType.LeftBrace, "Expect '{' before method body.");
                List<Stmt> body = ParseBlockStatements();
                Consume(TokenType.RightBrace, "Expect '}' after method body.");
                
                FunctionStmt method = new(false, firstIdentifier, memberName, 0, parameters, body);
                methods.Add(method);
            }
            else
            {
                // Field with user-defined type
                int arrayDimensions = ParseArrayDimensions();
                Consume(TokenType.Semicolon, "Expect ';' after field declaration.");
                VarStmt field = new(false, firstIdentifier, memberName, arrayDimensions, null);
                fields.Add(field);
            }
        }
        else
        {
            throw new Exception($"Unexpected identifier '{firstIdentifier.Lexeme}' in class body at position {firstIdentifier.Position}");
        }
    }

    #endregion

    #region Token Navigation

    /// <summary>
    /// Matches any of the given token types
    /// </summary>
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

    /// <summary>
    /// Consumes a token of the expected type or throws an error
    /// </summary>
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new Exception($"{message} Got '{Peek().Lexeme}' at position {Peek().Position}");
    }

    /// <summary>
    /// Checks if current token is of the given type
    /// </summary>
    private bool Check(TokenType type)
    {
        return !IsAtEnd() && Peek().Type == type;
    }

    /// <summary>
    /// Checks if next token is of the given type
    /// </summary>
    private bool CheckNext(TokenType type)
    {
        return current + 1 < tokens.Count && tokens[current + 1].Type == type;
    }

    /// <summary>
    /// Advances to the next token
    /// </summary>
    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    /// <summary>
    /// Checks if we're at the end of tokens
    /// </summary>
    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    /// <summary>
    /// Returns the current token
    /// </summary>
    private Token Peek()
    {
        return tokens[current];
    }

    /// <summary>
    /// Returns the previous token
    /// </summary>
    private Token Previous()
    {
        return tokens[current - 1];
    }

    #endregion
}