using PercentLang.Execution;

namespace PercentLang.Ast;

public class NodeCommandExecution : Node
{
    public required string CommandName { get; init; }

    public required List<Node> Arguments { get; init; }
    
    public NodeCommandExecution? NextInPipe { get; set; }

    public FilterType Filters { get; set; } = FilterType.StdOut;
}