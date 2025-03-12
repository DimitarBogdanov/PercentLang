using System.Diagnostics;
using System.Text;

namespace PercentLang.Execution;

public sealed class CommandExecution
{
    public CommandExecution(string command, string input, List<string> args)
    {
        CommandName = command;
        Arguments = args;

        StdIn = input;
        _out = new StringBuilder();
        _err = new StringBuilder();
    }
    
    public string CommandName { get; }

    public string StdIn  { get; }
    public string StdOut => _out.ToString();
    public string StdErr => _err.ToString();
    
    public Exception? RunException { get; private set; }
    
    public List<string> Arguments { get; }

    public int ResultCode { get; set; }
    
    public bool Muted { get; set; }

    private readonly StringBuilder _out;
    private readonly StringBuilder _err;

    public async Task<bool> Run()
    {
        try
        {
            await RunInternal();
            return true;
        }
        catch (Exception ex)
        {
            RunException = ex;
            return false;
        }
    }
    
    private async Task RunInternal()
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
            _err.AppendLine(e.Data);
            if (!Muted)
            {
                Console.WriteLine(e.Data);
            }
        };
        process.OutputDataReceived += (_, e) =>
        {
            _out.AppendLine(e.Data);
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