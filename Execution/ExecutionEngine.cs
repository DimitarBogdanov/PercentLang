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
    
    public CommandExecution? LastCmdExecution { get; set; }

    public async Task Execute()
    {
        FileExecutor executor = new(this);
        await executor.Execute();
    }

    public string GetLastCommandOutputRespectFilters()
    {
        if (LastCmdExecution == null)
        {
            return "";
        }

        FilterType filters = LastCmdExecution.Filters;

        if (filters.HasFlag(FilterType.StdErr))
        {
            return LastCmdExecution.StdErr;
        }

        if (filters.HasFlag(FilterType.Result))
        {
            return LastCmdExecution.ResultCode.ToString();
        }

        return LastCmdExecution.StdOut;
    }

    public Node GetVariableValueOrNullNode(string varName)
    {
        if (Variables.TryGetValue(varName, out Node? node))
        {
            return node;
        }

        return Node.Null;
    }
}