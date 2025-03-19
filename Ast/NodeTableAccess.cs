using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeTableAccess : NodeVarRef
{
    public required Node Index { get; init; }

    public override string GetStringRepresentation(ExecutionEngine engine)
    {
        Node var = engine.GetVariableValueOrNullNode(Name);
        if (var is not NodeTable tbl)
        {
            return Null.GetStringRepresentation(engine);
        }

        string? idx = Index.GetStringRepresentation(engine);
        if (idx == null)
        {
            return Null.GetStringRepresentation(engine);
        }

        return tbl.GetValue(idx).GetStringRepresentation(engine);
    }
}