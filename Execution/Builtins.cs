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

            CommandExecution exec = CommandExecution.Create(cmd.Engine, cmdName, "", args);
            exec.Run().Wait();
        }
    };
}