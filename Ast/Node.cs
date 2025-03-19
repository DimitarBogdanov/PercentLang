using PercentLang.Execution;

namespace PercentLang.Ast;

public abstract class Node
{
    public static readonly Node Null  = new NodeString { Value = "" };
    public static readonly Node True  = new NodeString { Value = "TRUE" };
    public static readonly Node False = new NodeString { Value = "FALSE" };

    public virtual string GetStringRepresentation(ExecutionEngine engine)
    {
        return Null.GetStringRepresentation(engine);
    }

    public bool IsTruthy(ExecutionEngine engine)
    {
        return !IsFalsy(engine);
    }

    public bool IsFalsy(ExecutionEngine engine)
    {
        return this == False
               || this == Null
               || GetStringRepresentation(engine) is "FALSE" or "";
    }
}