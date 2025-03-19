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
}