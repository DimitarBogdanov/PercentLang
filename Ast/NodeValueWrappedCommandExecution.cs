using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeValueWrappedCommandExecution : NodeCommandExecution
{
    public required Node Value { get; init; }

    public override string? GetStringRepresentation(ExecutionEngine engine)
    {
        return Value.GetStringRepresentation(engine);
    }
}