using PercentLang.Ast;
using PercentLang.Execution;
using PercentLang.Parsing;
using PercentLang.Tokenizing;

namespace PercentLang;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("percent | Usage: % filename");
            return;
        }

        string filePath = args[0];
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

        Tokenizer tok = new(fileText);
        List<Token> tokens = tok.Tokenize();

        foreach (Token t in tokens)
        {
            Console.WriteLine($"{t.Line,-2} {t.Type} :: {t.Value}");
        }

        Parser parser = new(tokens);
        NodeFile file = parser.Parse();

        Console.WriteLine($"{parser.Messages.Count} messages");
        foreach (string msg in parser.Messages)
        {
            Console.WriteLine(msg);
        }

        if (parser.Messages.Count != 0)
        {
            return;
        }

        ExecutionEngine engine = new(file);
        await engine.Execute();
    }
}