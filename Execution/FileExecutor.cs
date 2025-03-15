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
                    if (nva.Value is NodeCommandExecution nce)
                    {
                        await ExecCommand(nce);
                        _engine.Variables[nva.Var.Name] = new NodeString { Value = _engine.GetLastCommandOutputRespectFilters() };
                    }
                    else
                    {
                        _engine.Variables[nva.Var.Name] = nva.Value;
                    }
                    break;
                }
            }
        }
    }

    private async Task ExecCommand(NodeCommandExecution ex, string input = "")
    {
        CommandExecution exec;
        bool muted = ex.Filters.HasFlag(FilterType.Muted) || ex.NextInPipe != null;

        if (ex is NodeValueWrappedCommandExecution valueWrapped)
        {
            exec = new ValueWrappedCommandExecution(_engine, ex.Filters, valueWrapped.Value);
        }
        else
        {
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

            exec = CommandExecution.Create(_engine, ex.Filters, commandName, input, args);
        }
        
        exec.Muted = muted;

        await exec.Run();
        _engine.LastCmdExecution = exec;

        if (ex.NextInPipe is { } next)
        {
            if (ex.Filters.HasFlag(FilterType.PassAsArg))
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