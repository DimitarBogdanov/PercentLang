using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeTableAccess : NodeVarRef
{
    public required Node Index { get; init; }

    public override string? GetStringRepresentation(ExecutionEngine engine)
    {
        Node var = engine.GetVariableValueOrNullNode(Name);
        if (var is not NodeTable tbl)
        {
            return null;
        }

        string? idx = Index.GetStringRepresentation(engine);
        if (idx == null)
        {
            return null;
        }

        return tbl.GetValue(idx).GetStringRepresentation(engine);
    }
}