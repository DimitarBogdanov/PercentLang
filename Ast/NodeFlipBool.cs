using System.Globalization;
using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeFlipBool : Node
{
    public required Node InnerValue { get; init; }

    public override string GetStringRepresentation(ExecutionEngine engine)
    {
        return (InnerValue.IsTruthy(engine) ? False : True).GetStringRepresentation(engine);
    }
}