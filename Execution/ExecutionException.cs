namespace PercentLang.Execution;

public sealed class ExecutionException : Exception
{
    public ExecutionException(string message) : base(message)
    {
    }
}