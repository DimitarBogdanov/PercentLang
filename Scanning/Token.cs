namespace PercentLang.Scanning;

public sealed class Token
{
    public Token(string value, TokenType type, int line, BinOperatorType binOperatorType)
    {
        Value = value;
        Type = type;
        Line = line;
        BinOperatorType = binOperatorType;
    }

    public string Value { get; }

    public TokenType Type { get; }
    
    public BinOperatorType BinOperatorType { get; }
    
    public int Line { get; }
}