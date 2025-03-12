namespace PercentLang.Tokenizing;

public enum TokenType
{
    Default,
    String,
    
    Comma,
    Pipe,
    OpAdd,
    OpSub,
    OpMul,
    OpDiv,
    OpMod,
    
    LParen,
    RParen,
    LBracket,
    RBracket,
    LBrace,
    RBrace,
    
    OpSet,
    OpEq,
    OpLt,
    OpLeq,
    OpGt,
    OpGeq,
    
    KwIf,
    KwElse,
    KwElseIf,
    KwWhile,
    KwFor,
    KwUntil,
    
    Eof
}