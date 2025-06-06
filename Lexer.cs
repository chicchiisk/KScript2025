namespace Calculator;

public class Lexer
{
    private readonly string source;
    private int current = 0;
    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        ["int"] = TokenType.Int,
        ["float"] = TokenType.Float,
        ["char"] = TokenType.Char,
        ["bool"] = TokenType.Bool,
        ["void"] = TokenType.Void,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["for"] = TokenType.For,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["new"] = TokenType.New,
        ["return"] = TokenType.Return
    };

    public Lexer(string source)
    {
        this.source = source;
    }

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

    private void ScanToken(List<Token> tokens)
    {
        char c = Advance();
        switch (c)
        {
            case ' ':
            case '\r':
            case '\t':
            case '\n':
                break;
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
                AddToken(tokens, TokenType.Divide);
                break;
            case '%':
                AddToken(tokens, TokenType.Modulo);
                break;
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
            case '=':
                AddToken(tokens, Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '!':
                AddToken(tokens, Match('=') ? TokenType.BangEqual : throw new Exception($"Unexpected character '!' at position {current - 1}"));
                break;
            case '>':
                AddToken(tokens, Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '<':
                AddToken(tokens, Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '&':
                if (Match('&'))
                {
                    AddToken(tokens, TokenType.AndAnd);
                }
                else
                {
                    throw new Exception($"Unexpected character '&' at position {current - 1}");
                }
                break;
            case '|':
                if (Match('|'))
                {
                    AddToken(tokens, TokenType.OrOr);
                }
                else
                {
                    throw new Exception($"Unexpected character '|' at position {current - 1}");
                }
                break;
            case ';':
                AddToken(tokens, TokenType.Semicolon);
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
            case '\'':
                CharLiteral(tokens);
                break;
            default:
                if (char.IsDigit(c))
                {
                    Number(tokens);
                }
                else if (char.IsLetter(c) || c == '_')
                {
                    Identifier(tokens);
                }
                else
                {
                    throw new Exception($"Unexpected character '{c}' at position {current - 1}");
                }
                break;
        }
    }

    private void Number(List<Token> tokens)
    {
        int start = current - 1;
        
        while (char.IsDigit(Peek()))
            Advance();

        bool isFloat = false;
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            isFloat = true;
            Advance();
            while (char.IsDigit(Peek()))
                Advance();
        }

        // Check for 'f' suffix for float literals
        if (Peek() == 'f' || Peek() == 'F')
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

    private void Identifier(List<Token> tokens)
    {
        int start = current - 1;
        
        while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
            Advance();

        string text = source[start..current];
        TokenType type = keywords.GetValueOrDefault(text, TokenType.Identifier);
        AddToken(tokens, type);
    }

    private void CharLiteral(List<Token> tokens)
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

    private void AddToken(List<Token> tokens, TokenType type, object? literal = null)
    {
        int start = current - 1;
        if (type == TokenType.Identifier || type == TokenType.Int)
        {
            while (start > 0 && (char.IsLetterOrDigit(source[start - 1]) || source[start - 1] == '_'))
                start--;
        }
        string text = source[start..current];
        tokens.Add(new Token(type, text, literal, start));
    }

    private char Advance()
    {
        return source[current++];
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return source[current];
    }

    private char PeekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (source[current] != expected) return false;

        current++;
        return true;
    }

    private bool IsAtEnd()
    {
        return current >= source.Length;
    }
}