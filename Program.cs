using PercentLang.Ast;
using PercentLang.Parsing;
using PercentLang.Tokenizing;

namespace PercentLang;

public static class Program
{
    public static void Main(string[] args)
    {
        Tokenizer tok = new(    """
                                    $Name = "Peter"
                                    """);
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
    }
}