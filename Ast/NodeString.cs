namespace PercentLang.Ast;

public sealed class NodeString : Node
{
    public required string Value { get; init; }
}