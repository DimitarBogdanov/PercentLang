namespace PercentLang.Scanning;

public enum TokenType
{
    Default,
    String,
    
    Comma,
    Pipe,
    Colon,
    OpBinary,
    
    LParen,
    RParen,
    LBracket,
    RBracket,
    LBrace,
    RBrace,
    
    OpSet,
    OpEq,
    
    KwIf,
    KwElse,
    KwElseIf,
    KwWhile,
    KwFor,
    KwUntil,
    
    Eof
}