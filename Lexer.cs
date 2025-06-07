namespace Calculator;

/// <summary>
/// Lexical analyzer that converts source code into a sequence of tokens
/// </summary>
public class Lexer
{
    private readonly string source;
    private int current = 0;
    
    /// <summary>
    /// Keywords mapping for the scripting language
    /// </summary>
    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        // Type system
        ["int"] = TokenType.Int,
        ["float"] = TokenType.Float,
        ["char"] = TokenType.Char,
        ["bool"] = TokenType.Bool,
        ["void"] = TokenType.Void,
        ["string"] = TokenType.String,
        
        // Control flow
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["for"] = TokenType.For,
        ["while"] = TokenType.While,
        ["return"] = TokenType.Return,
        
        // Literals
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        
        // Object-oriented
        ["new"] = TokenType.New,
        ["struct"] = TokenType.Struct,
        ["class"] = TokenType.Class,
        
        // Module system
        ["import"] = TokenType.Import,
        ["export"] = TokenType.Export,
        ["as"] = TokenType.As,
        ["from"] = TokenType.From
    };

    public Lexer(string source)
    {
        this.source = source;
    }

    /// <summary>
    /// Scans the entire source code and returns a list of tokens
    /// </summary>
    public List<Token> ScanTokens()
    {
        List<Token> tokens = new();

        while (!IsAtEnd())
        {
            ScanToken(tokens);
        }

        tokens.Add(new Token(TokenType.EOF, "", null, current));
        return tokens;
    }

    /// <summary>
    /// Scans a single token from the current position
    /// </summary>
    private void ScanToken(List<Token> tokens)
    {
        char c = Advance();
        switch (c)
        {
            // Whitespace
            case ' ':
            case '\r':
            case '\t':
            case '\n':
                break;
                
            // Arithmetic operators
            case '+':
                AddToken(tokens, Match('+') ? TokenType.PlusPlus : TokenType.Plus);
                break;
            case '-':
                AddToken(tokens, Match('-') ? TokenType.MinusMinus : TokenType.Minus);
                break;
            case '*':
                AddToken(tokens, TokenType.Multiply);
                break;
            case '/':
                if (Match('/'))
                {
                    // Single-line comment - skip until end of line
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else
                {
                    AddToken(tokens, TokenType.Divide);
                }
                break;
            case '%':
                AddToken(tokens, TokenType.Modulo);
                break;
                
            // Delimiters
            case '(':
                AddToken(tokens, TokenType.LeftParen);
                break;
            case ')':
                AddToken(tokens, TokenType.RightParen);
                break;
            case '{':
                AddToken(tokens, TokenType.LeftBrace);
                break;
            case '}':
                AddToken(tokens, TokenType.RightBrace);
                break;
            case '[':
                AddToken(tokens, TokenType.LeftBracket);
                break;
            case ']':
                AddToken(tokens, TokenType.RightBracket);
                break;
            case ',':
                AddToken(tokens, TokenType.Comma);
                break;
            case '.':
                AddToken(tokens, TokenType.Dot);
                break;
            case ';':
                AddToken(tokens, TokenType.Semicolon);
                break;
                
            // Comparison and assignment operators
            case '=':
                AddToken(tokens, Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '!':
                if (!Match('='))
                    throw new Exception($"Unexpected character '!' at position {current - 1}");
                AddToken(tokens, TokenType.BangEqual);
                break;
            case '>':
                AddToken(tokens, Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '<':
                AddToken(tokens, Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
                
            // Logical operators
            case '&':
                if (!Match('&'))
                    throw new Exception($"Unexpected character '&' at position {current - 1}");
                AddToken(tokens, TokenType.AndAnd);
                break;
            case '|':
                if (!Match('|'))
                    throw new Exception($"Unexpected character '|' at position {current - 1}");
                AddToken(tokens, TokenType.OrOr);
                break;
                
            // Literals
            case '\'':
                ScanCharLiteral(tokens);
                break;
            case '"':
                ScanStringLiteral(tokens);
                break;
                
            // Complex tokens
            default:
                if (char.IsDigit(c))
                {
                    ScanNumber(tokens);
                }
                else if (char.IsLetter(c) || c == '_')
                {
                    ScanIdentifier(tokens);
                }
                else
                {
                    throw new Exception($"Unexpected character '{c}' at position {current - 1}");
                }
                break;
        }
    }

    /// <summary>
    /// Scans a numeric literal (integer or float)
    /// </summary>
    private void ScanNumber(List<Token> tokens)
    {
        int start = current - 1;
        
        while (char.IsDigit(Peek()))
            Advance();

        bool isFloat = false;
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            isFloat = true;
            Advance(); // consume '.'
            while (char.IsDigit(Peek()))
                Advance();
        }

        // Check for 'f' suffix for float literals
        if (Peek() is 'f' or 'F')
        {
            isFloat = true;
            Advance();
        }

        string text = source[start..current];
        
        // Parse based on type
        object value;
        if (isFloat)
        {
            // Remove 'f' or 'F' suffix if present
            string numberText = text.EndsWith('f') || text.EndsWith('F') ? text[0..^1] : text;
            value = float.Parse(numberText);
        }
        else
        {
            value = int.Parse(text);
        }
        
        AddToken(tokens, TokenType.Number, value);
    }

    /// <summary>
    /// Scans an identifier or keyword
    /// </summary>
    private void ScanIdentifier(List<Token> tokens)
    {
        int start = current - 1;
        
        while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
            Advance();

        string text = source[start..current];
        TokenType type = keywords.GetValueOrDefault(text, TokenType.Identifier);
        AddToken(tokens, type);
    }

    /// <summary>
    /// Scans a character literal with escape sequence support
    /// </summary>
    private void ScanCharLiteral(List<Token> tokens)
    {
        int start = current - 1;
        
        if (IsAtEnd())
            throw new Exception($"Unterminated character literal at position {start}");
        
        char value = Advance();
        
        // Handle escape sequences
        if (value == '\\')
        {
            if (IsAtEnd())
                throw new Exception($"Unterminated character literal at position {start}");
            
            char escaped = Advance();
            value = escaped switch
            {
                'n' => '\n',
                't' => '\t',
                'r' => '\r',
                '\\' => '\\',
                '\'' => '\'',
                _ => throw new Exception($"Invalid escape sequence '\\{escaped}' at position {current - 2}")
            };
        }
        
        if (!Match('\''))
            throw new Exception($"Unterminated character literal at position {start}");
        
        AddToken(tokens, TokenType.Number, value);
    }

    /// <summary>
    /// Scans a string literal with escape sequence support
    /// </summary>
    private void ScanStringLiteral(List<Token> tokens)
    {
        int start = current - 1;
        string value = "";
        
        while (!IsAtEnd() && Peek() != '"')
        {
            char c = Advance();
            if (c == '\\')
            {
                if (IsAtEnd())
                    throw new Exception($"Unterminated string literal at position {start}");
                
                char escaped = Advance();
                c = escaped switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\\' => '\\',
                    '"' => '"',
                    _ => throw new Exception($"Invalid escape sequence '\\{escaped}' at position {current - 2}")
                };
            }
            value += c;
        }
        
        if (IsAtEnd())
            throw new Exception($"Unterminated string literal at position {start}");
        
        // Consume the closing "
        Advance();
        
        AddToken(tokens, TokenType.StringLiteral, value);
    }

    /// <summary>
    /// Adds a token to the token list
    /// </summary>
    private void AddToken(List<Token> tokens, TokenType type, object? literal = null)
    {
        int start = current - 1;
        
        // For multi-character tokens, find the actual start
        if (type == TokenType.Identifier || type is TokenType.Int or TokenType.Float or TokenType.Char or TokenType.Bool or TokenType.Void)
        {
            while (start > 0 && (char.IsLetterOrDigit(source[start - 1]) || source[start - 1] == '_'))
                start--;
        }
        
        string text = source[start..current];
        tokens.Add(new Token(type, text, literal, start));
    }

    #region Helper Methods

    /// <summary>
    /// Advances to the next character and returns it
    /// </summary>
    private char Advance()
    {
        return source[current++];
    }

    /// <summary>
    /// Returns the current character without advancing
    /// </summary>
    private char Peek()
    {
        return IsAtEnd() ? '\0' : source[current];
    }

    /// <summary>
    /// Returns the next character without advancing
    /// </summary>
    private char PeekNext()
    {
        return current + 1 >= source.Length ? '\0' : source[current + 1];
    }

    /// <summary>
    /// Matches the expected character and advances if found
    /// </summary>
    private bool Match(char expected)
    {
        if (IsAtEnd() || source[current] != expected) return false;
        current++;
        return true;
    }

    /// <summary>
    /// Checks if we've reached the end of the source
    /// </summary>
    private bool IsAtEnd()
    {
        return current >= source.Length;
    }

    #endregion
}