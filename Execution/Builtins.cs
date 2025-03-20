using System.Globalization;
using PercentLang.Ast;

namespace PercentLang.Execution;

public static class Builtins
{
    public static Dictionary<string, Func<BuiltinCommandExecution, Action>> Factories = new()
    {
        ["echo"] = cmd => () =>
        {
            string output = String.Join(' ', cmd.Arguments);
            cmd.WriteStdOut(output);
            cmd.WriteStdOut("\n");
        },
        
        ["alias"] = cmd => () =>
        {
            if (cmd.Arguments.Count < 2)
            {
                cmd.WriteStdErr("alias requires two arguments");
                return;
            }

            string firstArg = cmd.Arguments[0];
            string secondArg = cmd.Arguments[1];

            cmd.Engine.Aliases[firstArg] = secondArg;
        },
        
        ["runcmd"] = cmd => () =>
        {
            if (cmd.Arguments.Count == 0)
            {
                cmd.WriteStdErr("runcmd required another command to be given as an arg");
                return;
            }

            string cmdName = cmd.Arguments[0];
            List<Node> args = cmd.NodeArguments.Skip(1).ToList();

            CommandExecution exec = CommandExecution.Create(cmd.Engine, FilterType.StdOut, cmdName, "", args);
            exec.Run().Wait();
        },
        
        ["runfn"] = cmd => () =>
        {
            if (cmd.Arguments.Count == 0)
            {
                throw new ExecutionException("No function supplied");
            }


            NodeFunc fn;
            if (cmd.NodeArguments.First() is NodeVarRef varRef)
            {
                Node value = varRef.GetValue(cmd.Engine);
                if (value is NodeFunc fn1)
                {
                    fn = fn1;
                }
                else
                {
                    throw new ExecutionException("Expected function for runfn");
                }
            }
            else if (cmd.NodeArguments.First() is NodeFunc fn1)
            {
                fn = fn1;
            }
            else
            {
                throw new ExecutionException("Expected function for runfn");
            }

            Node previousArgs = cmd.Engine.GetVariableValueOrNullNode("Args");
            NodeTable newArgs = new();
            Dictionary<string, Node> newArgsValues = [];
            for (int i = 1; i < cmd.NodeArguments.Count; i++)
            {
                newArgsValues[(i - 1).ToString()] = cmd.NodeArguments[i];
            }
            
            newArgs.Init(newArgsValues);

            cmd.Engine.Variables["Args"] = newArgs;

            cmd.Engine.CurrentFileExecutor.ExecuteBody(fn.Body).Wait();
            
            cmd.Engine.Variables["Args"] = previousArgs;
        },
        
        ["from_last_cmd"] = cmd => () =>
        {
            CommandExecution? last = cmd.Engine.LastCmdExecution;
            if (last == null)
            {
                cmd.SetResultCode(-1);
                return;
            }

            cmd.Muted = true;
            cmd.WriteStdOut(last.StdOut);
            cmd.WriteStdErr(last.StdErr);
            cmd.SetResultCode(last.ResultCode);
        },
        
        ["exit"] = cmd => () =>
        {
            string? result = cmd.Arguments.FirstOrDefault();
            if (result != null)
            {
                if (Int32.TryParse(result, CultureInfo.InvariantCulture, out int code))
                {
                    Environment.Exit(code);
                }
                else
                {
                    Environment.Exit(-20250316);
                }
            }
            
            Environment.Exit(0);
        },
        
        ["clear"] = _ => Console.Clear,
        
        ["bg"] = cmd => () =>
        {
            ConsoleColor color = ConsoleColor.Black;
            if (cmd.Arguments.Count != 0)
            {
                if (!Enum.TryParse(cmd.Arguments.First(), out color))
                {
                    throw new ExecutionException("Unknown color");
                    return;
                }
            }

            Console.BackgroundColor = color;
        },
        
        ["fg"] = cmd => () =>
        {
            ConsoleColor color = ConsoleColor.White;
            if (cmd.Arguments.Count != 0)
            {
                if (!Enum.TryParse(cmd.Arguments.First(), out color))
                {
                    throw new ExecutionException("Unknown color");
                    return;
                }
            }

            Console.ForegroundColor = color;
        }
    };
}