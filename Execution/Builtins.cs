namespace PercentLang.Execution;

public static class Builtins
{
    public static Dictionary<string, Func<BuiltinCommandExecution, Action>> Factories = new()
    {
        ["echo"] = cmd => () =>
        {
            string output = String.Join(' ', cmd.Arguments);
            cmd.WriteStdOut(output);
            cmd.WriteStdOut("\n");
        },
        
        ["alias"] = cmd => () =>
        {
            if (cmd.Arguments.Count < 2)
            {
                cmd.WriteStdErr("alias requires two arguments");
                return;
            }

            string firstArg = cmd.Arguments[0];
            string secondArg = cmd.Arguments[1];

            cmd.Engine.Aliases[firstArg] = secondArg;
        },
        
        ["runcmd"] = cmd => () =>
        {
            if (cmd.Arguments.Count == 0)
            {
                cmd.WriteStdErr("runcmd required another command to be given as an arg");
                return;
            }

            string cmdName = cmd.Arguments[0];
            List<string> args = cmd.Arguments.Skip(1).ToList();

            CommandExecution exec = CommandExecution.Create(cmd.Engine, FilterType.StdOut, cmdName, "", args);
            exec.Run().Wait();
        },
        
        ["from_last_cmd"] = cmd => () =>
        {
            CommandExecution? last = cmd.Engine.LastCmdExecution;
            if (last == null)
            {
                cmd.SetResultCode(-1);
                return;
            }

            cmd.Muted = true;
            cmd.WriteStdOut(last.StdOut);
            cmd.WriteStdErr(last.StdErr);
            cmd.SetResultCode(last.ResultCode);
        },
        
        ["exit"] = cmd => () =>
        {
            string? result = cmd.Arguments.FirstOrDefault();
            if (result != null)
            {
                if (Int32.TryParse(result, out int code))
                {
                    Environment.Exit(code);
                }
                else
                {
                    Environment.Exit(-20250316);
                }
            }
            
            Environment.Exit(0);
        },
    };
}