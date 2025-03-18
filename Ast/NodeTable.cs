namespace PercentLang.Ast;

public sealed class NodeTable : Node
{
    public NodeTable()
    {
        _values = new Dictionary<string, Node>();
    }
    
    private readonly Dictionary<string, Node> _values;

    public void Init(Dictionary<string, Node> values)
    {
        foreach (KeyValuePair<string,Node> pair in values)
        {
            _values.Add(pair.Key, pair.Value);
        }
    }

    public Node GetValue(string index)
    {
        return _values.GetValueOrDefault(index, Null);
    }
}