using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeValueWrappedCommandExecution : NodeCommandExecution
{
    public required Node Value { get; init; }
}