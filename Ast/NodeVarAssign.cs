namespace PercentLang.Ast;

public sealed class NodeVarAssign : Node
{
    public required NodeVarRef Var { get; init; }
    public required Node Value { get; init; }
}