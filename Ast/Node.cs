namespace PercentLang.Ast;

public abstract class Node
{
    public virtual string? GetStringRepresentation()
    {
        return null;
    }
}