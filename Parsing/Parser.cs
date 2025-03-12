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

    private int _currentLine;

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
        _currentLine = _tokens.CurrentLine();
        if (IsVarAssign())
        {
            return ParseVarAssign();
        }
        else if (IsNameRef())
        {
            return ParseCommand();
        }

        if (_currentLine == _tokens.CurrentLine())
        {
            throw new ParseException($"There are more tokens on line {_currentLine} than expected");
        }

        throw new ParseException("Expected command, if/loop, or variable assignment");
    }

    private Node ParseExpr(bool allowCommandExecutions)
    {
        if (IsString())
        {
            return ParseString();
        }

        if (IsNameRef())
        {
            if (allowCommandExecutions)
            {
                return ParseCommand();
            }

            string nameRef = ParseNameRef();
            return new NodeString
            {
                Value = nameRef
            };
        }

        if (IsVarRef())
        {
            return ParseVarRef();
        }

        throw new ParseException("Expected expression");
    }

    private NodeCommandExecution ParseCommand()
    {
        int line = _tokens.Current().Line;
        string name = ParseNameRef();

        List<Node> args = [];
        while (!_tokens.IsEof()
               && _tokens.CurrentLine() == line
               && !_tokens.CurrentIs(TokenType.Pipe, TokenType.Colon))
        {
            args.Add(ParseExpr(false));
        }

        NodeCommandExecution? nextCommand = null;
        bool passOutAsArgs = false;
        if (_tokens.CurrentIs(TokenType.Pipe, TokenType.Colon))
        {
            passOutAsArgs = _tokens.CurrentIs(TokenType.Colon);
            _tokens.Advance(); // skip the pipe
            nextCommand = ParseCommand();
        }

        return new NodeCommandExecution
        {
            CommandName = name,
            Arguments = args,
            NextInPipe = nextCommand,
            NextInPipePassStdOutAsArgs = passOutAsArgs
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
        Node value = ParseExpr(true);

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
        return _tokens.CurrentIs(TokenType.Default)
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
        return _tokens.CurrentIs(TokenType.Default)
               && !_tokens.Current().Value.StartsWith('$');
    }

    private string ParseNameRef()
    {
        StringBuilder name = new(_tokens.Current().Value);
        _tokens.Advance();

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