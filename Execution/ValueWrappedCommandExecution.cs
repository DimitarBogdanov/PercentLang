using PercentLang.Ast;

namespace PercentLang.Execution;

public sealed class ValueWrappedCommandExecution : CommandExecution
{
    public ValueWrappedCommandExecution(ExecutionEngine engine, FilterType filters, Node value)
        : base(engine, filters, "", "", [])
    {
        _value = value;
    }

    private readonly Node _value;
    
    protected override Task RunInternal()
    {
        string? str = _value.GetStringRepresentation(Engine);

        if (str != null)
        {
            Out.AppendLine(str);
            Err.AppendLine(str);
            ResultCode = 0;
        }
        else
        {
            Out.AppendLine("!ERROR");
            Err.AppendLine("!ERROR");
            ResultCode = -1;
            RunException = new Exception("Value did not have string representation");
        }

        return Task.CompletedTask;
    }
}