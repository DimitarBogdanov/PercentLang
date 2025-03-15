using System.Diagnostics;
using PercentLang.Ast;

namespace PercentLang.Execution;

public sealed class ProcessCommandExecution : CommandExecution
{
    public ProcessCommandExecution(ExecutionEngine engine, FilterType filters, string command, string input, List<string> args)
        : base(engine, filters, command, input, args)
    {
    }

    protected override async Task RunInternal()
    {
        ProcessStartInfo startInfo = new()
        {
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            FileName = CommandName
        };
        
        Arguments.ForEach(startInfo.ArgumentList.Add);

        Process process = new();
        process.StartInfo = startInfo;

        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += (_, e) =>
        {
            Err.AppendLine(e.Data);
            if (!Muted)
            {
                Console.WriteLine(e.Data);
            }
        };
        process.OutputDataReceived += (_, e) =>
        {
            Out.AppendLine(e.Data);
            if (!Muted)
            {
                Console.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await using (StreamWriter sw = process.StandardInput)
        {
            if (sw.BaseStream.CanWrite)
            {
                await sw.WriteLineAsync(StdIn);
            }
        }
        
        await process.WaitForExitAsync();

        ResultCode = process.ExitCode;
    }
}