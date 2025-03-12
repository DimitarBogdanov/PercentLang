using System.Text;
using PercentLang.Ast;
using PercentLang.Tokenizing;

namespace PercentLang.Parsing;

public sealed class Parser
{
    public Parser(List<Token> tokens)
    {
        _tokens = new TokenReaderUtil(tokens);
        Messages = new List<string>();
    }

    public List<string> Messages { get; }

    private readonly TokenReaderUtil _tokens;

    public NodeFile Parse()
    {
        NodeFile file = new();
        while (!_tokens.IsEof())
        {
            try
            {
                Node line = ParseLine();
                file.Commands.Add(line);
            }
            catch (ParseException ex)
            {
                Messages.Add(ex.Message);
                SkipToNextLine();
            }
        }

        return file;
    }

    private Node ParseLine()
    {
        if (IsVarAssign())
        {
            return ParseVarAssign();
        }
        else if (IsNameRef())
        {
            return ParseCommand();
        }

        throw new ParseException("Expected command, if/loop, or variable assignment");
    }

    private Node ParseExpr()
    {
        if (IsString())
        {
            return ParseString();
        }

        if (IsNameRef())
        {
            return ParseCommand();
        }

        throw new ParseException("Expected expression");
    }

    private Node ParseCommand()
    {
        string name = ParseNameRef();

        return new NodeCommandExecution
        {
            CommandName = name
        };
    }

    private bool IsVarAssign()
    {
        return Try(() =>
        {
            if (!IsVarRef())
            {
                return false;
            }

            ParseVarRef();

            return _tokens.CurrentIs(TokenType.OpSet);
        });
    }

    private NodeVarAssign ParseVarAssign()
    {
        NodeVarRef var = ParseVarRef();
        _tokens.Advance(); // skip '='
        Node value = ParseExpr();

        return new NodeVarAssign
        {
            Var = var,
            Value = value
        };
    }

    private bool IsString()
    {
        return _tokens.CurrentIs(TokenType.String);
    }

    private NodeString ParseString()
    {
        string val = _tokens.Current().Value;
        
        _tokens.Advance();
        
        return new NodeString
        {
            Value = val
        };
    }

    private bool IsVarRef()
    {
        return _tokens.CurrentIs(TokenType.Id)
               && _tokens.Current().Value.StartsWith('$');
    }

    private NodeVarRef ParseVarRef()
    {
        string varName = _tokens.Current().Value[1..];
        
        _tokens.Advance();
        
        return new NodeVarRef
        {
            Name = varName
        };
    }

    private bool IsNameRef()
    {
        return _tokens.CurrentIs(TokenType.Id)
               && !_tokens.Current().Value.StartsWith('$');
    }

    private string ParseNameRef()
    {
        StringBuilder name = new(_tokens.Current().Value);
        _tokens.Advance();
        while (_tokens.CurrentIs(TokenType.Period))
        {
            name.Append('.');
            _tokens.Advance();
            if (!_tokens.CurrentIs(TokenType.Id))
            {
                throw new ParseException("Expected identifier");
            }

            name.Append(_tokens.Current().Value);
            _tokens.Advance();
        }

        return name.ToString();
    }

    private bool Try(Func<bool> action)
    {
        BacktrackPoint bp = _tokens.GetBacktrackPoint();

        try
        {
            return action();
        }
        catch (ParseException)
        {
            return false;
        }
        finally
        {
            bp.RevertParser();
        }
    }

    private void SkipToNextLine()
    {
        int line = _tokens.Current().Line;
        while (!_tokens.IsEof() && _tokens.Current().Line == line)
        {
            _tokens.Advance();
        }
    }
}