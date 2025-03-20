namespace PercentLang.Scanning;

public static class BinOperatorPrecedenceExtensions
{
    public static int GetPrecedence(this BinOperatorType binOp)
    {
        return binOp switch
        {
            BinOperatorType.Or => 1,
            BinOperatorType.And => 2,
            BinOperatorType.Lt or BinOperatorType.Gt
                or BinOperatorType.Leq or BinOperatorType.Geq 
                or BinOperatorType.Neq or BinOperatorType.Eq => 3,
            
            BinOperatorType.Add or BinOperatorType.Sub or BinOperatorType.Concat => 4,
            BinOperatorType.Mul or BinOperatorType.Div or BinOperatorType.Mod => 5,
            
            _ => throw new ArgumentOutOfRangeException(nameof(binOp), "Unknown binary operator")
        };
    }

    public static bool IsBinaryOperator(this Token token)
    {
        return token.BinOperatorType != BinOperatorType.None;
    }
}