namespace PercentLang.Execution;

public sealed class Library
{
    public Library()
    {
        Functions = [];
    }
    
    public required string Name { get; init; }
    
    public Dictionary<string, ILibraryFn> Functions { get; }
}