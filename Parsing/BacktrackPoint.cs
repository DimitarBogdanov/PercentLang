namespace PercentLang.Parsing;

public class BacktrackPoint
{
    public BacktrackPoint(TokenReaderUtil util, int pos)
    {
        _util = util;
        _pos = pos;
    }

    private readonly TokenReaderUtil _util;
    private readonly int             _pos;

    public void RevertParser()
    {
        _util.SetPosition(_pos);
    }
}