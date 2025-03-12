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

                case NodeVarAssign nva:
                {
                    _engine.Variables[nva.Var.Name] = nva.Value;
                    break;
                }
            }
        }
    }

    private async Task ExecCommand(NodeCommandExecution ex, string input = "")
    {
        bool muted = ex.NextInPipe != null;
        
        List<string> args = ex.Arguments
            .Select(x => x.GetStringRepresentation(_engine))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        string commandName = ex.CommandName;
        if (_engine.Aliases.TryGetValue(commandName, out string? aliasedName))
        {
            commandName = aliasedName;
        }

        CommandExecution exec = CommandExecution.Create(_engine, commandName, input, args);
        exec.Muted = muted;

        await exec.Run();

        if (ex.NextInPipe is { } next)
        {
            if (ex.NextInPipePassStdOutAsArgs)
            {
                next.Arguments.Add(new NodeString
                {
                    Value = exec.StdOut
                });
            }
            await ExecCommand(next, exec.StdOut);
        }
    }
}