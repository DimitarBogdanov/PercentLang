namespace PercentLang.Ast;

public sealed class NodeWhile : Node
{
    public required Node Condition { get; init; } 
    
    public required List<Node> Body { get; init; }
}