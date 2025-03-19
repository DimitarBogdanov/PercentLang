using System.Globalization;
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

        engine.Libraries.Add(new Library
        {
            Name = "Math",
            Functions =
            {
                ["Round"] = ILibraryFn.Wrap(exec =>
                {
                    if (exec.Arguments.Count == 0)
                    {
                        throw new ExecutionException("Math.Round expects one argument");
                    }

                    string arg = exec.Arguments[0];
                    if (!Double.TryParse(arg, CultureInfo.InvariantCulture, out double res))
                    {
                        throw new ExecutionException("Math.Round expects number");
                    }

                    res = Math.Round(res);
                    
                    exec.WriteStdOut(res.ToString(CultureInfo.InvariantCulture));
                }),
                
                ["Floor"] = ILibraryFn.Wrap(exec =>
                {
                    if (exec.Arguments.Count == 0)
                    {
                        throw new ExecutionException("Math.Floor expects one argument");
                    }

                    string arg = exec.Arguments[0];
                    if (!Double.TryParse(arg, CultureInfo.InvariantCulture, out double res))
                    {
                        throw new ExecutionException("Math.Floor expects number");
                    }

                    res = Math.Floor(res);
                    
                    exec.WriteStdOut(res.ToString(CultureInfo.InvariantCulture));
                }),
                
                ["Ceil"] = ILibraryFn.Wrap(exec =>
                {
                    if (exec.Arguments.Count == 0)
                    {
                        throw new ExecutionException("Math.Ceil expects one argument");
                    }

                    string arg = exec.Arguments[0];
                    if (!Double.TryParse(arg, CultureInfo.InvariantCulture, out double res))
                    {
                        throw new ExecutionException("Math.Ceil expects number");
                    }

                    res = Math.Ceiling(res);
                    
                    exec.WriteStdOut(res.ToString(CultureInfo.InvariantCulture));
                }),
                
                ["Sqrt"] = ILibraryFn.Wrap(exec =>
                {
                    if (exec.Arguments.Count == 0)
                    {
                        throw new ExecutionException("Math.Sqrt expects one argument");
                    }

                    string arg = exec.Arguments[0];
                    if (!Double.TryParse(arg, CultureInfo.InvariantCulture, out double res))
                    {
                        throw new ExecutionException("Math.Sqrt expects number");
                    }

                    res = Math.Sqrt(res);
                    
                    exec.WriteStdOut(res.ToString(CultureInfo.InvariantCulture));
                }),
                
                ["Abs"] = ILibraryFn.Wrap(exec =>
                {
                    if (exec.Arguments.Count == 0)
                    {
                        throw new ExecutionException("Math.Abs expects one argument");
                    }

                    string arg = exec.Arguments[0];
                    if (!Double.TryParse(arg, CultureInfo.InvariantCulture, out double res))
                    {
                        throw new ExecutionException("Math.Abs expects number");
                    }

                    res = Math.Abs(res);
                    
                    exec.WriteStdOut(res.ToString(CultureInfo.InvariantCulture));
                }),
                
                ["Min"] = ILibraryFn.Wrap(exec =>
                {
                    if (exec.Arguments.Count == 0)
                    {
                        throw new ExecutionException("Math.Min expects at least one argument");
                    }

                    List<double> list = new(exec.Arguments.Count);
                    int i = 1;
                    foreach (string arg in exec.Arguments)
                    {
                        if (!Double.TryParse(arg, CultureInfo.InvariantCulture, out double num))
                        {
                            throw new ExecutionException($"Argument {i} for Math.Min isn't a number");
                        }

                        i++;
                        
                        list.Add(num);
                    }

                    double res = list.Min();
                    
                    exec.WriteStdOut(res.ToString(CultureInfo.InvariantCulture));
                }),
                
                ["Max"] = ILibraryFn.Wrap(exec =>
                {
                    if (exec.Arguments.Count == 0)
                    {
                        throw new ExecutionException("Math.Max expects at least one argument");
                    }

                    List<double> list = new(exec.Arguments.Count);
                    int i = 1;
                    foreach (string arg in exec.Arguments)
                    {
                        if (!Double.TryParse(arg, CultureInfo.InvariantCulture, out double num))
                        {
                            throw new ExecutionException($"Argument {i} for Math.Max isn't a number");
                        }

                        i++;
                        
                        list.Add(num);
                    }

                    double res = list.Max();
                    
                    exec.WriteStdOut(res.ToString(CultureInfo.InvariantCulture));
                }),
            }
        });
    }
}