namespace PercentLang.Ast;

public sealed class NodeVarRef : Node
{
    public required string Name { get; init; }
}