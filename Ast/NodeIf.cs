namespace PercentLang.Ast;

public sealed class NodeIf : Node
{
    public sealed record Branch(Node Condition, List<Node> Body);
    
    public required Branch Main { get; init; }

    public List<Branch> ElseIfs { get; init; } = [];
    
    public List<Node>? Else { get; init; }
}