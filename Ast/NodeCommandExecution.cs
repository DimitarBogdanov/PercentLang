using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeCommandExecution : Node
{
    public required string CommandName { get; init; }

    public List<Node> Arguments { get; } = [];
    
    public NodeCommandExecution? NextInPipe { get; set; }
}