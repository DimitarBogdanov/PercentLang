namespace PercentLang.Scanning;

public sealed class Token
{
    public Token(string value, TokenType type, int line)
    {
        Value = value;
        Type = type;
        Line = line;
    }

    public string Value { get; }

    public TokenType Type { get; }
    
    public int Line { get; }
}