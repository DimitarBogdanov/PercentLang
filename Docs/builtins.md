# Builtin commands in %

### Syntax of this file
```
command <required> [optional]
command <required> [optional ...] # ... means repeat
```
---

### echo
Prints to the terminal.
```
echo [arg ...]
```

### alias
Creates a binding from one name to another.
The name *a* will be rewritten as *b* in execution.
If *b* is not given, `alias` echoes the current meaning of *a*.
```
alias <a> [b]
```

### runcmd
Runs a command, ignoring aliases and builtin commands.
```
alias mkdir echo
runcmd mkdir myDir # creates directory, doesn't echo
```

### from_last_cmd
Does nothing. Used to retrieve values from the last executed command. Useful if you need more than one result from it via a filter.
```
$StdOut = ipconfig
$StdErr = from_last_cmd : stderr
$Res = from_last_cmd : result
```

### exit
Exits the shell. *code* must be a number, if absent 0 is returned. If not a number, -20250316 is returned.
```
exit [code]
```