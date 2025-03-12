using PercentLang.Ast;

namespace PercentLang.Execution;

public sealed class ExecutionEngine
{
    public ExecutionEngine(NodeFile file)
    {
        File = file;
        
        Variables = new Dictionary<string, Node>();
        Aliases = new Dictionary<string, string>();
    }
    
    public NodeFile File { get; }

    public Dictionary<string, Node> Variables { get; }
    
    public Dictionary<string, string> Aliases { get; }

    public async Task Execute()
    {
        FileExecutor executor = new(this);
        await executor.Execute();
    }
}