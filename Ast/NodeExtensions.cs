using System.Globalization;
using PercentLang.Execution;

namespace PercentLang.Ast;

public static class NodeExtensions
{
    public static bool IsNumber(this Node node, ExecutionEngine engine, out double value)
    {
        string str = node.GetStringRepresentation(engine);
        
        if (Double.TryParse(str, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = 0;
        return false;
    }
}