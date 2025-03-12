using System.Text;

namespace PercentLang.Tokenizing;

public sealed class Tokenizer
{
    private enum State
    {
        Default,
        String,
        Number
    }

    public Tokenizer(string input)
    {
        _tokens = new LinkedList<Token>();
        _value = new StringBuilder();
        _input = input.Replace("\r", "") + '\n';
    }

    private readonly LinkedList<Token> _tokens;
    private readonly StringBuilder     _value;
    private readonly string            _input;

    private int   _line  = 1;
    private int   _pos   = -1;
    private State _state = State.Default;

    public List<Token> Tokenize()
    {
        bool isEscape = false;

        while (!IsEof())
        {
            _pos++;
            char current = PeekChar();

            if (current == '\n')
            {
                if (isEscape)
                {
                    continue;
                }
                
                PushTokInferType();
                _line++;
                continue;
            }
            
            if (isEscape)
            {
                isEscape = false;
                _value.Append(current);
                continue;
            }

            if (current == '\\')
            {
                isEscape = true;
                continue;
            }

            if (current == '"' && !isEscape)
            {
                if (_state == State.String)
                {
                    PushTok(TokenType.String);
                    _state = State.Default;
                }
                else
                {
                    PushTokInferType();
                    _state = State.String;
                }

                continue;
            }

            if (_value.Length == 0 && Char.IsDigit(current))
            {
                PushTokInferType();
                _value.Append(current);
                _state = State.Number;
                continue;
            }
            
            if (_state == State.Number)
            {
                if (Char.IsDigit(current) || (current == '.' && !_value.ToString().Contains('.')))
                {
                    _value.Append(current);
                }
                else 
                {
                    PushTokInferType();
                    _state = State.Default;
                }

                continue;
            }

            if (Char.IsWhiteSpace(current))
            {
                PushTokInferType();
                continue;
            }

            char next = PeekChar(1);
            switch (current)
            {
                case '(':
                    PushTok(TokenType.LParen, "(");
                    continue;
                case ')':
                    PushTok(TokenType.RParen, ")");
                    continue;
                
                case '[':
                    PushTok(TokenType.LBracket, "[");
                    continue;
                case ']':
                    PushTok(TokenType.RBracket, "]");
                    continue;
                
                case '{':
                    PushTok(TokenType.LBrace, "{");
                    continue;
                case '}':
                    PushTok(TokenType.RBrace, "}");
                    continue;
                
                case '=' when next is '=':
                    _pos++;
                    PushTok(TokenType.OpEq, "==");
                    continue;
                case '=':
                    PushTok(TokenType.OpSet, "=");
                    continue;
                
                case '<' when next is '=':
                    _pos++;
                    PushTok(TokenType.OpLeq, "<=");
                    continue;
                case '<':
                    PushTok(TokenType.OpLt, "<");
                    continue;
                
                case '>' when next is '=':
                    _pos++;
                    PushTok(TokenType.OpGeq, ">=");
                    continue;
                case '>':
                    PushTok(TokenType.OpGt, ">");
                    continue;
                
                case '.':
                    PushTok(TokenType.Period, ".");
                    continue;
                
                case ',':
                    PushTok(TokenType.Comma, ",");
                    continue;
                
                case '+':
                    PushTok(TokenType.OpAdd, "+");
                    continue;
                
                case '-':
                    PushTok(TokenType.OpSub, "-");
                    continue;
                
                case '*':
                    PushTok(TokenType.OpMul, "*");
                    continue;
                
                case '/':
                    PushTok(TokenType.OpDiv, "/");
                    continue;
                
                case '%':
                    PushTok(TokenType.OpMod, "%");
                    continue;
            }

            _value.Append(current);
        }

        if (isEscape || _state == State.String)
        {
            // Unhandled situation!
            // TODO Err
        }

        return _tokens.ToList();
    }

    private void PushTok(TokenType type)
    {
        string value = _value.ToString().Trim();
        _tokens.AddLast(new Token(value, type, _line));

        _value.Clear();
    }

    private void PushTok(TokenType type, string expressValue)
    {
        if (_value.Length != 0)
        {
            throw new InvalidOperationException("Cannot use express value when there is already a store value");
        }

        _value.Append(expressValue);
        PushTok(type);
    }

    private void PushTokInferType()
    {
        string trimmedVal = _value.ToString().Trim();
        if (trimmedVal.Length == 0)
        {
            // Nothing to push!
            return;
        }
        
        TokenType t = _state switch
        {
            State.String => TokenType.String,
            State.Number => TokenType.Number,

            _ => TokenType.Id
        };

        if (t == TokenType.Id)
        {
            t = trimmedVal switch
            {
                "if"     => TokenType.KwIf,
                "else"   => TokenType.KwElse,
                "elseif" => TokenType.KwElseIf,
                "while"  => TokenType.KwWhile,
                "for"    => TokenType.KwFor,
                "until"  => TokenType.KwUntil,
                
                _ => TokenType.Id
            };
        }

        PushTok(t);
    }

    private char PeekChar(int skipCount = 0)
    {
        int idx = _pos + skipCount;
        return idx >= _input.Length ? '\0' : _input[idx];
    }

    private bool IsEof()
    {
        return _pos >= _input.Length;
    }
}