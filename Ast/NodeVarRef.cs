using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeVarRef : Node
{
    public required string Name { get; init; }

    public override string? GetStringRepresentation(ExecutionEngine engine)
    {
        return engine.Variables.TryGetValue(Name, out Node? value)
            ? value.GetStringRepresentation(engine)
            : "";
    }
}