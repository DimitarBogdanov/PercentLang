﻿using PercentLang.Ast;
using PercentLang.Execution;
using PercentLang.Parsing;
using PercentLang.Tokenizing;

namespace PercentLang;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Tokenizer tok = new(    """
                                    $Name = "Test"
                                    $Cmd = "echo"
                                    alias say $Cmd
                                    say $Name
                                    runcmd echo hi
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

        if (parser.Messages.Count != 0)
        {
            return;
        }

        ExecutionEngine engine = new(file);
        await engine.Execute();
    }
}