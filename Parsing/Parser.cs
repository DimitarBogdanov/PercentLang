using System.Text;
using PercentLang.Ast;
using PercentLang.Execution;
using PercentLang.Scanning;

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
        return ParseExprRhs(ParseExprPrimary(allowCommandExecutions), 0, allowCommandExecutions);
    }

    private Node ParseExprRhs(Node lhs, int minPrecedence, bool allowCommandExecutions)
    {
        Token lookahead = _tokens.Current();
        if (lookahead.IsBinaryOperator() && lookahead.BinOperatorType.GetPrecedence() >= minPrecedence)
        {
            BinOperatorType op = lookahead.BinOperatorType;
            _tokens.Advance();

            Node rhs = ParseExprPrimary(allowCommandExecutions);

            lookahead = _tokens.Current();

            while (lookahead.IsBinaryOperator() && lookahead.BinOperatorType.GetPrecedence() > op.GetPrecedence())
            {
                rhs = ParseExprRhs(rhs, op.GetPrecedence() + 1, allowCommandExecutions);
                lookahead = _tokens.Current();
            }

            lhs = new NodeBinaryOp { Op = op, Lhs = lhs, Rhs = rhs };
        }

        return lhs;
    }

    private Node ParseExprPrimary(bool allowCommandExecutions)
    {
        Node returnValue;
        
        if (IsString())
        {
            returnValue = ParseString();
        }
        else if (IsTable())
        {
            returnValue = ParseTable();
        }
        else if (IsNameRef())
        {
            if (allowCommandExecutions)
            {
                returnValue = ParseCommand();
            }
            else
            {
                string nameRef = ParseNameRef();
                
                returnValue = new NodeString
                {
                    Value = nameRef
                };
            }
        }
        else if (IsVarRef())
        {
            returnValue = ParseVarRef();
        }
        else
        {
            throw new ParseException("Expected expression");
        }

        if (allowCommandExecutions && (IsPipe() || _tokens.CurrentIs(TokenType.Colon)))
        {
            FilterType filters = ParseFiltersIfAny();
            
            NodeCommandExecution next = ParsePipe();
            returnValue = new NodeValueWrappedCommandExecution
            {
                CommandName = "",
                Arguments = [],
                Value = returnValue,
                Filters = filters,
                NextInPipe = next
            };
        }

        return returnValue;
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

        FilterType filters = ParseFiltersIfAny();

        NodeCommandExecution? nextCommand = null;
        if (IsPipe())
        {
            nextCommand = ParsePipe();
        }

        return new NodeCommandExecution
        {
            CommandName = name,
            Arguments = args,
            NextInPipe = nextCommand,
            Filters = filters
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

    private bool IsTable()
    {
        return _tokens.CurrentIs(TokenType.LBrace);
    }

    private NodeTable ParseTable()
    {
        _tokens.Advance(); // skip '{'

        Dictionary<string, Node> values = [];
        int listIdx = 0;
        while (!_tokens.CurrentIs(TokenType.RBrace) && !_tokens.IsEof())
        {
            if (_tokens.CurrentIs(TokenType.String) && _tokens.Peek().Type == TokenType.OpSet)
            {
                // dict value
                string key = _tokens.Current().Value;
                _tokens.Advance(); // skip key
                _tokens.Advance(); // skip value
                Node value = ParseExpr(false);

                values[key] = value;
            }
            else
            {
                Node value = ParseExpr(false);
                values[listIdx.ToString()] = value;
                listIdx++;
            }
            

            if (_tokens.CurrentIs(TokenType.Comma))
            {
                _tokens.Advance();
            }
            else
            {
                break;
            }
        }

        if (_tokens.CurrentIs(TokenType.RBrace))
        {
            _tokens.Advance();
        }

        NodeTable res = new();
        res.Init(values);

        return res;
    }

    private NodeTableAccess ParseTableAccess(NodeVarRef tableRef)
    {
        _tokens.Advance(); // skip '['
        Node index = ParseExpr(false);
        _tokens.Advance(); // skip ']'

        return new NodeTableAccess { Name = tableRef.Name, Index = index };
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
        NodeVarRef value = new NodeVarRef
        {
            Name = varName
        };

        if (_tokens.CurrentIs(TokenType.LBracket))
        {
            return ParseTableAccess(value);
        }

        return value;
    }

    private bool IsPipe()
    {
        return _tokens.CurrentIs(TokenType.Pipe);
    }

    private NodeCommandExecution ParsePipe()
    {
        _tokens.Advance();
        return ParseCommand();
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

    private FilterType ParseFiltersIfAny()
    {
        FilterType filters = FilterType.None;
        if (_tokens.CurrentIs(TokenType.Colon))
        {
            _tokens.Advance();
            do
            {
                if (_tokens.CurrentIs(TokenType.Comma))
                {
                    _tokens.Advance();
                }
                
                FilterType res = _tokens.Current().Value switch
                {
                    "stdout" => FilterType.StdOut,
                    "stderr" => FilterType.StdErr,
                    "result" => FilterType.Result,
                    "muted"  => FilterType.Muted,
                    "as_arg" => FilterType.PassAsArg,
                    
                    _ => (FilterType)(-1)
                };

                if (res == (FilterType)(-1))
                {
                    throw new ParseException($"Invalid filter '{_tokens.Current().Value}'");
                }

                filters |= res;
                
                _tokens.Advance();
            } while (_tokens.CurrentIs(TokenType.Comma));
        }

        return filters;
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