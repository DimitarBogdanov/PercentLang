namespace PercentLang.Ast;

public sealed class NodeTable : Node
{
    public NodeTable()
    {
        _values = new Dictionary<string, Node>();
    }
    
    private readonly Dictionary<string, Node> _values;

    public void InitList(List<Node> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            _values[i.ToString()] = values[i];
        }
    }

    public Node GetValue(string index)
    {
        return _values.GetValueOrDefault(index, Null);
    }
}