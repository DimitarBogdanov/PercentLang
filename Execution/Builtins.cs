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
        }
    };
}