namespace Calculator;

public enum TokenType
{
    Number,
    Plus,
    Minus,
    Multiply,
    Divide,
    Modulo,
    PlusPlus,
    MinusMinus,
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Comma,
    
    Identifier,
    Equal,
    EqualEqual,
    BangEqual,
    Greater,
    GreaterEqual,
    Less,
    LessEqual,
    AndAnd,
    OrOr,
    Semicolon,
    
    If,
    Else,
    For,
    Int,
    Float,
    Char,
    Bool,
    Void,
    True,
    False,
    New,
    Return,
    
    EOF
}

public record Token(TokenType Type, string Lexeme, object? Literal, int Position);