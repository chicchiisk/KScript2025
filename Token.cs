namespace Calculator;

/// <summary>
/// Represents all possible token types in the scripting language
/// </summary>
public enum TokenType
{
    // Literals and identifiers
    Number,
    Identifier,
    True,
    False,
    
    // Arithmetic operators
    Plus,
    Minus,
    Multiply,
    Divide,
    Modulo,
    
    // Increment/decrement operators
    PlusPlus,
    MinusMinus,
    
    // Delimiters and punctuation
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Comma,
    Semicolon,
    Dot,
    
    // Assignment and comparison operators
    Equal,
    EqualEqual,
    BangEqual,
    Greater,
    GreaterEqual,
    Less,
    LessEqual,
    
    // Logical operators
    AndAnd,
    OrOr,
    
    // Control flow keywords
    If,
    Else,
    For,
    Return,
    
    // Type system keywords
    Int,
    Float,
    Char,
    Bool,
    Void,
    
    // Object-oriented keywords
    New,
    Struct,
    
    // Module system keywords
    Import,
    Export,
    As,
    From,
    
    // End of file marker
    EOF
}

/// <summary>
/// Represents a single token in the source code with position information
/// </summary>
public record Token(TokenType Type, string Lexeme, object? Literal, int Position);

/// <summary>
/// Extension methods for Token operations
/// </summary>
public static class TokenExtensions
{
    /// <summary>
    /// Checks if the token represents a primitive type
    /// </summary>
    public static bool IsPrimitiveType(this Token token)
    {
        return token.Type is TokenType.Int or TokenType.Float or TokenType.Char or TokenType.Bool or TokenType.Void;
    }
    
    /// <summary>
    /// Checks if the token represents a binary operator
    /// </summary>
    public static bool IsBinaryOperator(this Token token)
    {
        return token.Type is TokenType.Plus or TokenType.Minus or TokenType.Multiply or TokenType.Divide or TokenType.Modulo
            or TokenType.EqualEqual or TokenType.BangEqual or TokenType.Greater or TokenType.GreaterEqual
            or TokenType.Less or TokenType.LessEqual or TokenType.AndAnd or TokenType.OrOr;
    }
}