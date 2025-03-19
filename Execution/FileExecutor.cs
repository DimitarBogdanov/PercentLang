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
            try
            {
                await ExecuteCommand(cmd);
            }
            catch (ExecutionException e)
            {
                await Console.Error.WriteLineAsync(e.Message);
                return;
            }
        }
    }

    private async Task ExecuteBody(List<Node> body)
    {
        foreach (Node cmd in body)
        {
            await ExecuteCommand(cmd);
        }
    }

    private async Task ExecuteCommand(Node cmd)
    {
        switch (cmd)
        {
            case NodeCommandExecution ex:
            {
                await ExecCommand(ex);
                break;
            }

            case NodeIf ifStat:
            {
                if (ifStat.Main.Condition.IsTruthy(_engine))
                {
                    await ExecuteBody(ifStat.Main.Body);
                }
                else
                {
                    foreach (NodeIf.Branch branch in ifStat.ElseIfs)
                    {
                        if (branch.Condition.IsTruthy(_engine))
                        {
                            await ExecuteBody(branch.Body);
                            return;
                        }
                    }

                    if (ifStat.Else != null)
                    {
                        await ExecuteBody(ifStat.Else);
                    }
                }
                
                break;
            }

            case NodeWhile whileLoop:
            {
                while (whileLoop.Condition.IsTruthy(_engine))
                {
                    await ExecuteBody(whileLoop.Body);
                }
                break;
            }

            case NodeVarAssign nva:
            {
                Node value;
                if (nva.Value is NodeCommandExecution nce)
                {
                    await ExecCommand(nce);
                    value = new NodeString { Value = _engine.GetLastCommandOutputRespectFilters() };
                }
                else
                {
                    value = new NodeString { Value = nva.Value.GetStringRepresentation(_engine) };
                }

                if (nva.Var is NodeTableAccess tableAccess)
                {
                    Node tbl = _engine.GetVariableValueOrNullNode(tableAccess.Name);
                    if (tbl is not NodeTable table)
                    {
                        throw new ExecutionException("Attempted to set a value of non-table");
                    }
                        
                    string? stringRep = tableAccess.Index.GetStringRepresentation(_engine);
                    if (stringRep == null)
                    {
                        throw new ExecutionException("Table index was evaluated as NULL");
                    }

                    table.SetValue(stringRep, nva.Value);
                }
                else
                {
                    _engine.Variables[nva.Var.Name] = value;
                }
                break;
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
        if (_engine.LastCmdExecution is { RunException: {} re })
        {
            throw new ExecutionException(re.Message);
        }

        if (ex.NextInPipe is { } next)
        {
            if (ex.Filters.HasFlag(FilterType.PassAsArg))
            {
                next.Arguments.Add(new NodeString
                {
                    Value = _engine.GetLastCommandOutputRespectFilters()
                });
            }
            await ExecCommand(next, exec.StdOut);
        }
    }
}