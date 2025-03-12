using PercentLang.Ast;

namespace PercentLang.Execution;

public sealed class FileExecutor
{
    public FileExecutor(ExecutionEngine engine)
    {
        _engine = engine;
    }

    private readonly ExecutionEngine _engine;

    public async Task Execute()
    {
        foreach (Node cmd in _engine.File.Commands)
        {
            switch (cmd)
            {
                case NodeCommandExecution ex:
                {
                    await ExecCommand(ex);
                    break;
                }
            }
        }
    }

    private async Task ExecCommand(NodeCommandExecution ex, string input = "")
    {
        bool muted = ex.NextInPipe != null;
        
        List<string> args = ex.Arguments
            .Select(x => x.GetStringRepresentation())
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
        
        CommandExecution exec = new(ex.CommandName, input, args)
        {
            Muted = muted
        };

        await exec.Run();

        if (ex.NextInPipe is { } next)
        {
            await ExecCommand(next, exec.StdOut);
        }
    }
}