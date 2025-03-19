﻿namespace PercentLang.Scanning;

public static class BinOperatorPrecedenceExtensions
{
    public static int GetPrecedence(this BinOperatorType binOp)
    {
        return binOp switch
        {
            BinOperatorType.Add or BinOperatorType.Sub => 0,
            BinOperatorType.Mul or BinOperatorType.Div or BinOperatorType.Mod => 1,
            BinOperatorType.Lt or BinOperatorType.Gt or BinOperatorType.Leq or BinOperatorType.Geq => 2,
            
            _ => throw new ArgumentOutOfRangeException(nameof(binOp), "Unknown binary operator")
        };
    }

    public static bool IsBinaryOperator(this Token token)
    {
        return token.BinOperatorType != BinOperatorType.None;
    }
}