using PercentLang.Execution;

namespace PercentLang.Ast;

public abstract class Node
{
    public static readonly Node Null = new NodeString { Value = "" };
    
    public virtual string? GetStringRepresentation(ExecutionEngine engine)
    {
        return null;
    }
}