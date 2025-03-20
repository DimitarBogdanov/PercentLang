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

    /// <returns>Whether the execution was interrupted (break/return)</returns>
    public async Task ExecuteBody(List<Node> body)
    {
        foreach (Node cmd in body)
        {
            if (cmd is NodeBreakLoop)
            {
                throw new LoopInterruptionException();
            }
            
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
                try
                {
                    while (whileLoop.Condition.IsTruthy(_engine))
                    {
                        await ExecuteBody(whileLoop.Body);
                    }
                }
                catch (LoopInterruptionException)
                {
                    // discard
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
                else if (nva.Value is NodeTable or NodeFunc)
                {
                    value = nva.Value;
                }
                else
                {
                    value = new NodeString { Value = nva.Value.GetStringRepresentation(_engine) };
                }
                
                nva.Var.SetValue(_engine, value);
                
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
            List<Node> args = [];
            foreach (Node arg in ex.Arguments)
            {
                Node useValue;
                if (arg is NodeVarRef varRef)
                {
                    useValue = varRef.GetValue(_engine);
                }
                else
                {
                    useValue = arg;
                }
                args.AddRange(useValue.GetExplodedVersion());
            }
            
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