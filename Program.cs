using PercentLang.Tokens;

namespace PercentLang;

public static class Program
{
    public static void Main(string[] args)
    {
        Tokenizer tok = new(    """
                                    repeat {
                                    	echo $NumValue
                                    	$NumValue = $NumValue / 2
                                    } until $NumValue <= 1
                                    """);
        List<Token> tokens = tok.Tokenize();

        foreach (Token t in tokens)
        {
            Console.WriteLine($"{t.Type} :: {t.Value}");
        }
    }
}