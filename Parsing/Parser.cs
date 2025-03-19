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
        
        _file = new NodeFile();
    }

    public List<string> Messages { get; }

    private readonly TokenReaderUtil _tokens;
    private readonly NodeFile _file;

    private int _loopDepth;
    private int _commandExecWrapperDepth;
    private int _currentLine;

    public NodeFile Parse()
    {
        while (!_tokens.IsEof())
        {
            try
            {
                Node line = ParseLine();
                _file.Commands.Add(line);
            }
            catch (ParseException ex)
            {
                Messages.Add(ex.Message);
                SkipToNextLine();
            }
        }

        return _file;
    }

    private Node ParseLine()
    {
        _currentLine = _tokens.CurrentLine();
        if (IsVarAssign())
        {
            return ParseVarAssign();
        }
        
        if (IsNameRef())
        {
            return ParseCommand();
        }

        if (IsIf())
        {
            return ParseIf();
        }

        if (IsWhile())
        {
            return ParseWhile();
        }

        if (IsRepeat())
        {
            return ParseRepeat();
        }

        if (IsFor())
        {
            return ParseFor();
        }

        if (IsBreak())
        {
            return ParseBreak();
        }

        if (_currentLine == _tokens.CurrentLine())
        {
            throw new ParseException($"There are more tokens on line {_currentLine} than expected");
        }

        throw new ParseException("Expected command, if/loop, or variable assignment");
    }

    private Node ParseExpr()
    {
        return ParseExprRhs(ParseExprPrimary(), 0);
    }

    private Node ParseExprRhs(Node lhs, int minPrecedence)
    {
        Token lookahead = _tokens.Current();
        if (lookahead.IsBinaryOperator() && lookahead.BinOperatorType.GetPrecedence() >= minPrecedence)
        {
            BinOperatorType op = lookahead.BinOperatorType;
            _tokens.Advance();

            Node rhs = ParseExprPrimary();

            lookahead = _tokens.Current();

            while (lookahead.IsBinaryOperator() && lookahead.BinOperatorType.GetPrecedence() > op.GetPrecedence())
            {
                rhs = ParseExprRhs(rhs, op.GetPrecedence() + 1);
                lookahead = _tokens.Current();
            }

            lhs = new NodeBinaryOp { Op = op, Lhs = lhs, Rhs = rhs };
        }

        return lhs;
    }

    private Node ParseExprPrimary()
    {
        Node returnValue;

        if (IsCommandWrapper())
        {
            returnValue = ParseCommandWrapper();
        }
        else if (IsString())
        {
            returnValue = ParseString();
        }
        else if (IsTable())
        {
            returnValue = ParseTable();
        }
        else if (IsNameRef())
        {
            string nameRef = ParseNameRef();
                
            returnValue = new NodeString
            {
                Value = nameRef
            };
        }
        else if (IsVarRef())
        {
            returnValue = ParseVarRef();
        }
        else if (IsNegateNumber())
        {
            returnValue = ParseNegateNumber();
        }
        else
        {
            throw new ParseException("Expected expression");
        }

        if (IsPipe() || _tokens.CurrentIs(TokenType.Colon))
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

    private bool IsCommandWrapper()
    {
        return _tokens.CurrentIs(TokenType.Hash);
    }

    private NodeVarRef ParseCommandWrapper()
    {
        _tokens.Advance(); // skip '#'

        if (!_tokens.CurrentIs(TokenType.LParen))
        {
            throw new ParseException("Expected #( command ) syntax");
        }
        
        _tokens.Advance(); // skip '('

        _commandExecWrapperDepth++;
        NodeCommandExecution exec = ParseCommand();
        _commandExecWrapperDepth--;

        if (!_tokens.CurrentIs(TokenType.RParen))
        {
            throw new ParseException("Expected ) to close command execution");
        }
        
        _tokens.Advance(); // skip ')'

        string varName = $"!CmdWrapper_{Guid.NewGuid().ToString()}";
        NodeVarRef varRef = new NodeVarRef { Name = varName };
        
        _file.Commands.Add(exec);
        _file.Commands.Add(new NodeVarAssign
        {
            Var = varRef,
            Value = new NodeCommandExecution
            {
                CommandName = "from_last_cmd",
                Filters = exec.Filters,
                Arguments = []
            }
        });

        return varRef;
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
            if (_tokens.CurrentIs(TokenType.RParen) && _commandExecWrapperDepth != 0)
            {
                break;
            }
            
            args.Add(ParseExpr());
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
        Node value = ParseExpr();

        return new NodeVarAssign
        {
            Var = var,
            Value = value
        };
    }

    private bool IsIf()
    {
        return _tokens.CurrentIs(TokenType.KwIf);
    }

    private NodeIf ParseIf()
    {
        _tokens.Advance(); // skip 'if'
        Node mainCondition = ParseExpr();
        
        if (!_tokens.CurrentIs(TokenType.LBrace))
        {
            throw new ParseException("Expected if body");
        }
        List<Node> mainBody = ParseInstructionBlock();

        List<NodeIf.Branch> branches = [];
        while (_tokens.CurrentIs(TokenType.KwElseIf))
        {
            _tokens.Advance(); // skip 'elseif'
            Node cond = ParseExpr();
            if (!_tokens.CurrentIs(TokenType.LBrace))
            {
                throw new ParseException("Expected elseif body");
            }

            List<Node> body = ParseInstructionBlock();
            branches.Add(new NodeIf.Branch(cond, body));
        }

        List<Node>? elseBody = null;
        if (_tokens.CurrentIs(TokenType.KwElse))
        {
            _tokens.Advance(); // skip 'else'
            if (!_tokens.CurrentIs(TokenType.LBrace))
            {
                throw new ParseException("Expected else body");
            }

            elseBody = ParseInstructionBlock();
        }

        return new NodeIf
        {
            Main = new NodeIf.Branch(mainCondition, mainBody),
            ElseIfs = branches,
            Else = elseBody
        };
    }

    private bool IsWhile()
    {
        return _tokens.CurrentIs(TokenType.KwWhile);
    }

    private NodeWhile ParseWhile()
    {
        _tokens.Advance(); // skip 'while'

        Node condition = ParseExpr();

        if (!_tokens.CurrentIs(TokenType.LBrace))
        {
            throw new ParseException("Expected while body");
        }

        _loopDepth++;
        List<Node> body = ParseInstructionBlock();
        _loopDepth--;

        return new NodeWhile { Condition = condition, Body = body };
    }
    
    private bool IsRepeat()
    {
        return _tokens.CurrentIs(TokenType.KwRepeat);
    }

    private NodeWhile ParseRepeat()
    {
        int line = _tokens.CurrentLine();
        _tokens.Advance(); // skip 'repeat'

        if (!_tokens.CurrentIs(TokenType.LBrace))
        {
            throw new ParseException("Expected repeat body");
        }

        _loopDepth++;
        List<Node> body = ParseInstructionBlock();
        _loopDepth--;
        
        if (!_tokens.CurrentIs(TokenType.KwUntil))
        {
            throw new ParseException($"Expected 'until' (for 'repeat' on line {line})");
        }
        
        _tokens.Advance(); // skip 'until'

        Node condition = ParseExpr();

        body.Add(
            new NodeIf
            {
                Main = new NodeIf.Branch(condition, [ new NodeBreakLoop() ])
            }
        );

        return new NodeWhile { Condition = Node.True, Body = body };
    }

    private bool IsFor()
    {
        return _tokens.CurrentIs(TokenType.KwFor);
    }

    private NodeWhile ParseFor()
    {
        _tokens.Advance(); // skip 'for'

        NodeVarAssign initialVarAssign = ParseVarAssign();
        NodeVarRef loopVarRef = initialVarAssign.Var;
        Node step = new NodeString { Value = "1" };

        if (!_tokens.CurrentIs(TokenType.Comma))
        {
            throw new ParseException("Expected comma after assignment for variable");
        }
        
        _tokens.Advance(); // skip first comma
        Node goal = ParseExpr();

        if (_tokens.CurrentIs(TokenType.Comma))
        {
            _tokens.Advance();
            step = ParseExpr();
        }
        
        if (!_tokens.CurrentIs(TokenType.LBrace))
        {
            throw new ParseException("Expected while body");
        }

        _loopDepth++;
        List<Node> body = ParseInstructionBlock();
        _loopDepth--;
        
        // Check if we should exit
        body.Add(new NodeIf
        {
            Main = new NodeIf.Branch(
                Condition: new NodeBinaryOp { Lhs = loopVarRef, Rhs = goal, Op = BinOperatorType.Eq },
                Body: [
                    new NodeBreakLoop()
                ]
            )
        });
        
        // Value increment
        body.Add(new NodeVarAssign
        {
            Var = loopVarRef,
            Value = new NodeBinaryOp { Lhs = loopVarRef, Rhs = step, Op = BinOperatorType.Add }
        });

        return new NodeWhile
        {
            Condition = Node.True, Body =
            [
                initialVarAssign,
                new NodeWhile
                {
                    Condition = Node.True,
                    Body = body
                },
                new NodeBreakLoop()
            ]
        };
    }

    private List<Node> ParseInstructionBlock()
    {
        _tokens.Advance(); // skip '{'

        List<Node> res = [];
        while (!_tokens.IsEof() && !_tokens.CurrentIs(TokenType.RBrace))
        {
            Node line = ParseLine();
            res.Add(line);
        }
        
        _tokens.Advance(); // skip '}'
        return res;
    }

    private bool IsBreak()
    {
        return _tokens.CurrentIs(TokenType.KwBreak);
    }

    private NodeBreakLoop ParseBreak()
    {
        if (_loopDepth == 0)
        {
            throw new ParseException("Break statement should not exist outside of a loop");
        }

        _tokens.Advance(); // skip 'break'
        return new NodeBreakLoop();
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

    private bool IsNegateNumber()
    {
        return _tokens.Current().BinOperatorType == BinOperatorType.Sub;
    }

    private NodeNegateNumber ParseNegateNumber()
    {
        _tokens.Advance(); // skip '-'
        Node value = ParseExprPrimary();
        return new NodeNegateNumber { InnerValue = value };
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
                Node value = ParseExpr();

                values[key] = value;
            }
            else
            {
                Node value = ParseExpr();
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
        Node index = ParseExpr();
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
        int loopDepth = _loopDepth;

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
            _loopDepth = loopDepth;
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