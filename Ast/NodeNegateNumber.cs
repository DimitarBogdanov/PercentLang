using System.Globalization;
using PercentLang.Execution;

namespace PercentLang.Ast;

public sealed class NodeNegateNumber : Node
{
    public required Node InnerValue { get; init; }

    public override string GetStringRepresentation(ExecutionEngine engine)
    {
        if (!InnerValue.IsNumber(engine, out double value))
        {
            throw new ExecutionException("Unary minus value must be a number");
        }

        return (-value).ToString(CultureInfo.InvariantCulture);
    }
}