using PercentLang.Scanning;

namespace PercentLang.Parsing;

public sealed class TokenReaderUtil
{
    public TokenReaderUtil(List<Token> tokens)
    {
        _tokens = tokens;
        _maxLine = tokens.Last().Line;
    }

    private readonly List<Token> _tokens;
    private readonly int _maxLine;

    private int _pos;

    public Token Current()
    {
        return Peek(0);
    }

    public int CurrentLine()
    {
        return Current().Line;
    }

    public Token Peek(int with = 1)
    {
        int idx = _pos + with;
        if (idx >= _tokens.Count)
        {
            return GetEofTok();
        }

        return _tokens[idx];
    }
    
    public bool CurrentIs(TokenType type)
    {
        return Current().Type == type;
    }
    
    public bool CurrentIs(params TokenType[] type)
    {
        return type.Contains(Current().Type);
    }

    public bool NextIs(TokenType type)
    {
        return Peek(1).Type == type;
    }
    
    public void Advance()
    {
        _pos++;
    }

    public BacktrackPoint GetBacktrackPoint()
    {
        return new BacktrackPoint(this, _pos);
    }

    public void SetPosition(int pos)
    {
        _pos = pos;
    }

    public bool IsEof()
    {
        return _pos >= _tokens.Count;
    }

    private Token GetEofTok()
    {
        return new Token("<EOF>", TokenType.Eof, _maxLine);
    }
}