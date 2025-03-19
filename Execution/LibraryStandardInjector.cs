using System.Text;

namespace PercentLang.Execution;

public static class LibraryStandardInjector
{
    public static void Inject(ExecutionEngine engine)
    {
        // Lib.CLI
        engine.Libraries.Add(new Library
        {
            Name = "CLI",
            Functions =
            {
                ["Write"] = ILibraryFn.Wrap(exec =>
                {
                    string output = String.Join(' ', exec.Arguments);
                    Console.Write(output);
                }),
                
                ["WriteLine"] = ILibraryFn.Wrap(exec =>
                {
                    string output = String.Join(' ', exec.Arguments);
                    Console.WriteLine(output);
                }),
                
                ["ReadLine"] = ILibraryFn.Wrap(exec =>
                {
                    string input = Console.ReadLine() ?? "";
                    input = input.Trim();
                    exec.WriteStdOut(input);
                }),
                
                ["ReadChar"] = ILibraryFn.Wrap(exec =>
                {
                    int ch = Console.Read();
                    if (ch < 0)
                    {
                        ch = 0;
                    }
                    exec.WriteStdOut(((char)ch).ToString());
                }),
                
                ["ReadPassword"] = ILibraryFn.Wrap(exec =>
                {
                    string pass = "";
                    ConsoleKey key;
                    do
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;

                        if (key == ConsoleKey.Backspace && pass.Length > 0)
                        {
                            Console.Write("\b \b");
                            pass = pass[..^1];
                        }
                        else if (!char.IsControl(keyInfo.KeyChar))
                        {
                            Console.Write("*");
                            pass += keyInfo.KeyChar;
                        }
                    } while (key != ConsoleKey.Enter);

                    Console.WriteLine();
                    exec.WriteStdOut(pass);
                }),
            }
        });
    }
}