using System.Globalization;
using PercentLang.Execution;

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

    public void SetValue(string index, Node value)
    {
        _values[index] = value;
    }

    public override IEnumerable<Node> GetExplodedVersion()
    {
        int i = 0;
        while (_values.TryGetValue(i.ToString(CultureInfo.InvariantCulture), out Node? value))
        {
            foreach (Node node in value.GetExplodedVersion())
            {
                yield return node;
            }

            i++;
        }
    }

    public override string GetStringRepresentation(ExecutionEngine engine)
    {
        return $"Table{{{_values.Count} value(s)}}";
    }
}