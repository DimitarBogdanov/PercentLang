using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeTableAccess : Node
{
    public required NodeVarRef VarRef { get; init; }
    public required Node       Index  { get; init; }

    public override string? GetStringRepresentation(ExecutionEngine engine)
    {
        Node var = engine.GetVariableValueOrNullNode(VarRef.Name);
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