using System.Globalization;
using PercentLang.Execution;
using PercentLang.Scanning;

namespace PercentLang.Ast;

public sealed class NodeBinaryOp : Node
{
    public required BinOperatorType Op  { get; init; }
    public required Node            Lhs { get; init; }
    public required Node            Rhs { get; init; }

    public Node GetValue(ExecutionEngine engine)
    {
        switch (Op)
        {
            case BinOperatorType.Concat:
            {
                string lhs = Lhs.GetStringRepresentation(engine);
                string rhs = Rhs.GetStringRepresentation(engine);
                return new NodeString { Value = lhs + rhs };
            }

            case BinOperatorType.Add:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return new NodeString { Value = (left + right).ToString(CultureInfo.InvariantCulture) };
                }

                throw new ExecutionException("Addition requires two numbers on both sides");
            }

            case BinOperatorType.Sub:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return new NodeString { Value = (left - right).ToString(CultureInfo.InvariantCulture) };
                }

                throw new ExecutionException("Subtraction requires two numbers on both sides");
            }
            
            case BinOperatorType.Mul:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return new NodeString { Value = (left * right).ToString(CultureInfo.InvariantCulture) };
                }

                throw new ExecutionException("Multiplication requires two numbers on both sides");
            }
            
            case BinOperatorType.Div:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    if (right == 0)
                    {
                        throw new ExecutionException("Cannot divide by 0");
                    }
                    
                    return new NodeString { Value = (left / right).ToString(CultureInfo.InvariantCulture) };
                }

                throw new ExecutionException("Division requires two numbers on both sides");
            }
            
            case BinOperatorType.Mod:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    if (right == 0)
                    {
                        throw new ExecutionException("Cannot divide by 0");
                    }
                    
                    return new NodeString { Value = (left % right).ToString(CultureInfo.InvariantCulture) };
                }

                throw new ExecutionException("Modulo requires two numbers on both sides");
            }
            
            case BinOperatorType.Lt:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return left < right ? True : False;
                }

                throw new ExecutionException("Math operators require two numbers on both sides");
            }
            
            case BinOperatorType.Gt:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return left > right ? True : False;
                }

                throw new ExecutionException("Math operators require two numbers on both sides");
            }
            
            case BinOperatorType.Leq:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return left <= right ? True : False;
                }

                throw new ExecutionException("Math operators require two numbers on both sides");
            }
            
            case BinOperatorType.Geq:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return left >= right ? True : False;
                }

                throw new ExecutionException("Math operators require two numbers on both sides");
            }

            case BinOperatorType.Eq:
            {
                if (Lhs.IsNumber(engine, out double left) && Rhs.IsNumber(engine, out double right))
                {
                    return Math.Abs(left - right) < 0.05 ? True : False;
                }

                return Lhs.GetStringRepresentation(engine) == Rhs.GetStringRepresentation(engine) ? True : False;
            }


            case BinOperatorType.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(Op));
        }
    }

    public override string GetStringRepresentation(ExecutionEngine engine)
    {
        return GetValue(engine).GetStringRepresentation(engine);
    }
}