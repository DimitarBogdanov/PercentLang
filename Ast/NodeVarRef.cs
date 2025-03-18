using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeVarRef : Node
{
    public required string Name { get; init; }

    public override string? GetStringRepresentation(ExecutionEngine engine)
    {
        return engine
            .GetVariableValueOrNullNode(Name)
            .GetStringRepresentation(engine);
    }
}