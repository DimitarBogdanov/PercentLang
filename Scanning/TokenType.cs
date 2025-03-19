namespace PercentLang.Scanning;

public enum TokenType
{
    Default,
    String,
    
    Comma,
    Pipe,
    Colon,
    OpSet,
    OpBinary,
    
    LParen,
    RParen,
    LBracket,
    RBracket,
    LBrace,
    RBrace,
    
    KwIf,
    KwElse,
    KwElseIf,
    KwWhile,
    KwFor,
    KwRepeat,
    KwUntil,
    KwBreak,
    
    Eof
}