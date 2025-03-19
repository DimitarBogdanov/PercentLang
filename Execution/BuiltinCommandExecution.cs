using PercentLang.Ast;

namespace PercentLang.Execution;

public sealed class BuiltinCommandExecution : CommandExecution
{
    public BuiltinCommandExecution(ExecutionEngine engine, FilterType filters, string command, string input, List<Node> args)
        : base(engine, filters, command, input, args)
    {
    }
    
    public void WriteStdErr(string x)
    {
        Err.Append(x);
        if (!Muted)
        {
            Console.Error.Write(x);
        }
    }

    public void WriteStdOut(string x)
    {
        Out.Append(x);
        if (!Muted)
        {
            Console.Write(x);
        }
    }

    public void SetResultCode(int code)
    {
        ResultCode = code;
    }

    protected override Task RunInternal()
    {
        return Task.Run(Builtins.Factories[CommandName](this));

    }
}