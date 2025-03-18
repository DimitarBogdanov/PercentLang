# Filters in %

Filters are attached to commands via `:` and dictate how their results are outputted and processed by the language. Filters can be stacked by comma separation.

### stdout
Default. Retrieves the command's standard output.
```
$TheOuput = ping 127.0.0.1 : stdout
```

### stderr
Retrieves the command's standard error.
```
$TheError = always_errors : stderr
```

### result
Retrieves the command's exit code.
```
$Res = ipconfig : result
```

### muted
Prevents the output of the program from being printed to the standard output of the % process.
```
ipconfig : muted
```

### as_arg
Used in chain operations: whatever the program outputs is passed as an argument to the next program in the pipe. This operation respects other filters.
```
# Pings 127.0.0.1
echo 127.0.0.1 : as_arg | ping

# Prints the result code
ipconfig : result, as_arg | echo "Result code is"
```