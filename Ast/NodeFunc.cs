using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeFunc : Node
{
    public required List<Node> Body { get; init; }

    public override string GetStringRepresentation(ExecutionEngine engine)
    {
        return "func{}";
    }
}