namespace PercentLang.Tokens;

public enum TokenType
{
    Id,
    String,
    Number,
    LineBreak,
    
    Period,
    Comma,
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
}