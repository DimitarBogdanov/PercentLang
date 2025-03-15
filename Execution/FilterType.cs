namespace PercentLang.Execution;

[Flags]
public enum FilterType
{
    /// <summary>
    /// Treated the same as <see cref="StdOut"/>
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Gets the stdout output of the executed command.
    /// </summary>
    StdOut = 1,

    /// <summary>
    /// Gets the stderr output of the executed command.
    /// </summary>
    StdErr = 2,

    /// <summary>
    /// Gets the result of the executed command.
    /// </summary>
    Result = 4,

    /// <summary>
    /// Passes the value of the execution as an argument to the next command in the pipe chain.
    /// Otherwise, it's passed as stdin.
    /// </summary>
    PassAsArg = 8,
    
    /// <summary>
    /// Requires the file executor to mute the stdout of the process.
    /// </summary>
    Muted = 16,
}