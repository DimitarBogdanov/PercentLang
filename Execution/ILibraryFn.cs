namespace PercentLang.Execution;

public interface ILibraryFn
{
    internal class LibraryFnWrapper : ILibraryFn
    {
        internal LibraryFnWrapper(Action<LibraryCommandExecution> exec)
        {
            _exec = exec;
        }

        private readonly Action<LibraryCommandExecution> _exec;

        public void Run(LibraryCommandExecution execution)
        {
            _exec(execution);
        }
    }
    
    public void Run(LibraryCommandExecution execution);

    public static ILibraryFn Wrap(Action<LibraryCommandExecution> action)
    {
        return new LibraryFnWrapper(action);
    }
}