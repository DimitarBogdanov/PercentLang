using PercentLang.Scanning;

namespace PercentLang.Ast;

public sealed class NodeBinaryOp : Node
{
    public required BinOperatorType Op { get; init; }
    public required Node Lhs { get; init; }
    public required Node Rhs { get; init; }
}