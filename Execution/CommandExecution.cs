﻿using System.Diagnostics;
using System.Text;
using PercentLang.Ast;

namespace PercentLang.Execution;

public abstract class CommandExecution
{
    protected CommandExecution(ExecutionEngine engine, FilterType filters, string command, string input, List<string> args)
    {
        Engine = engine;
        
        CommandName = command;
        Arguments = args;

        Filters = filters;

        StdIn = input;
        Out = new StringBuilder();
        Err = new StringBuilder();
    }
    
    public string CommandName { get; }

    public string StdIn  { get; }
    public string StdOut => Out.ToString();
    public string StdErr => Err.ToString();
    
    public Exception? RunException { get; protected set; }
    
    public List<string> Arguments { get; }
    
    public FilterType Filters { get; }

    public int ResultCode { get; set; }
    
    public bool Muted { get; set; }

    protected readonly StringBuilder Out;
    protected readonly StringBuilder Err;
    
    public ExecutionEngine Engine { get; }

    public static CommandExecution Create(ExecutionEngine engine, FilterType filters, string command, string input, List<string> args)
    {
        return Builtins.Factories.ContainsKey(command)
            ? new BuiltinCommandExecution(engine, filters, command, input, args)
            : new ProcessCommandExecution(engine, filters, command, input, args);
    }
    
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

    protected abstract Task RunInternal();
}