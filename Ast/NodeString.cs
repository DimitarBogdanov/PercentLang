using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeString : Node
{
    public required string Value { get; init; }

    public override string? GetStringRepresentation(ExecutionEngine engine)
    {
        return Value;
    }
}