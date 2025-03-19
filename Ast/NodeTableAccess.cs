using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeTableAccess : NodeVarRef
{
    public required Node Index { get; init; }

    public override Node GetValue(ExecutionEngine engine)
    {
        Node var = base.GetValue(engine);
        if (var is not NodeTable tbl)
        {
            return Null;
        }
        
        string idx = Index.GetStringRepresentation(engine);

        return tbl.GetValue(idx);
    }

    public override void SetValue(ExecutionEngine engine, Node value)
    {
        Node var = base.GetValue(engine);
        if (var is not NodeTable tbl)
        {
            throw new ExecutionException($"Tried to set index in non-table {Name}");
        }
        
        string idx = Index.GetStringRepresentation(engine);

        tbl.SetValue(engine, idx, value);
    }
}