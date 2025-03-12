using PercentLang.Execution;

namespace PercentLang.Ast;

public abstract class Node
{
    public virtual string? GetStringRepresentation(ExecutionEngine engine)
    {
        return null;
    }
}