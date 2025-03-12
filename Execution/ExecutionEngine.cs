using PercentLang.Ast;

namespace PercentLang.Execution;

public sealed class ExecutionEngine
{
    public ExecutionEngine(NodeFile file)
    {
        File = file;
    }
    
    public NodeFile File { get; }

    public async Task Execute()
    {
        FileExecutor executor = new(this);
        await executor.Execute();
    }
}