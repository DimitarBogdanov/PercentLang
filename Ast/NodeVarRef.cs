using PercentLang.Execution;

namespace PercentLang.Ast;

public class NodeVarRef : Node
{
    public required string Name { get; init; }

    public override string GetStringRepresentation(ExecutionEngine engine)
    {
        return GetValue(engine).GetStringRepresentation(engine);
    }

    public virtual Node GetValue(ExecutionEngine engine)
    {
        return engine.GetVariableValueOrNullNode(Name);
    }

    public virtual void SetValue(ExecutionEngine engine, Node value)
    {
        if (value == Null)
        {
            engine.Variables.Remove(Name);
            return;
        }
        
        engine.Variables[Name] = value;
    }
}