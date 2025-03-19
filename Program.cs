using PercentLang.Ast;
using PercentLang.Execution;
using PercentLang.Parsing;
using PercentLang.Scanning;

namespace PercentLang;

public static class Program
{
    public static bool ShowDiagnostics { get; private set; } = false;

    public static async Task Main(string[] args)
    {
        string initFilePath = Path.Combine(Environment.CurrentDirectory, "__init__.%");
        string filePath = "";
        if (args.All(x => x.StartsWith("--")))
        {
            if (!File.Exists(initFilePath))
            {
                Console.WriteLine("percent | Usage: % filename (--diagnostics)");
                return;
            }

            filePath = initFilePath;
        }

        if (filePath == "")
        {
            filePath = args[0];
        }

        if (!File.Exists(filePath))
        {
            if (File.Exists($"{filePath}.%"))
            {
                filePath += ".%";
            }
            else
            {
                Console.WriteLine("percent | File not found");
                return;
            }
        }

        string fileText = await File.ReadAllTextAsync(filePath);

        ShowDiagnostics = args.Contains("--diagnostics");

        Scanner scanner = new(fileText);
        List<Token> tokens = scanner.Tokenize();

        Parser parser = new(tokens);
        NodeFile file = parser.Parse();

        if (ShowDiagnostics)
        {
            foreach (Token t in tokens)
            {
                Console.WriteLine($"{t.Line,-2} {t.Type} :: {t.Value}");
            }
        }

        if (parser.Messages.Count == 1)
        {
            Console.WriteLine("1 message");
        }
        else if (parser.Messages.Count != 0)
        {
            Console.WriteLine($"{parser.Messages.Count} messages");
        }

        foreach (string msg in parser.Messages)
        {
            Console.WriteLine(msg);
        }

        ExecutionEngine engine = new(file);
        await engine.Execute();
    }
}