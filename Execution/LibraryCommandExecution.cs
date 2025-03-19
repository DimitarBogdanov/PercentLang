using PercentLang.Ast;

namespace PercentLang.Execution;

public sealed class LibraryCommandExecution : CommandExecution
{
    public LibraryCommandExecution(ExecutionEngine engine, FilterType filters, string library, string command, string input, List<Node> args)
        : base(engine, filters, command, input, args)
    {
        Muted = true;
        _library = library;
    }

    private readonly string _library;
    
    public void WriteStdErr(string x)
    {
        Err.Append(x);
    }

    public void WriteStdOut(string x)
    {
        Out.Append(x);
    }

    public void SetResultCode(int code)
    {
        ResultCode = code;
    }

    protected override Task RunInternal()
    {
        Library lib = Engine.Libraries.First(x => x.Name == _library);
        ILibraryFn fn = lib.Functions[CommandName];
        
        fn.Run(this);
        return Task.CompletedTask;
    }
}