using System.Globalization;
using System.Text;

namespace PercentLang.Scanning;

public sealed class Scanner
{
    private enum State
    {
        Default,
        DefaultEscaped,
        String,
    }

    public Scanner(string input)
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
                if (_value.Length == 0)
                {
                    _state = State.DefaultEscaped;
                }
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

            if (_state == State.String)
            {
                _value.Append(current);
                continue;
            }

            if (Char.IsWhiteSpace(current))
            {
                PushTokInferType();
                continue;
            }

            // If a default token is escaped, don't look for punctuation within it
            // This allows the user to write e.g.  \g++ instead of g\+\+
            if (_state != State.DefaultEscaped)
            {
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
                    
                    case '^':
                        PushBinOpTok(BinOperatorType.Xor, "^");
                        continue;
                    
                    case '&' when next is '&':
                        _pos++;
                        PushBinOpTok(BinOperatorType.And, "&&");
                        continue;
                    
                    case '|' when next is '|':
                        _pos++;
                        PushBinOpTok(BinOperatorType.Or, "||");
                        continue;

                    case '=' when next is '=':
                        _pos++;
                        PushBinOpTok(BinOperatorType.Eq, "==");
                        continue;
                    case '=':
                        PushTok(TokenType.OpSet, "=");
                        continue;

                    case '<' when next is '=':
                        _pos++;
                        PushBinOpTok(BinOperatorType.Leq, "<=");
                        continue;
                    case '<':
                        PushBinOpTok(BinOperatorType.Lt, "<");
                        continue;

                    case '>' when next is '=':
                        _pos++;
                        PushBinOpTok(BinOperatorType.Geq, ">=");
                        continue;
                    case '>':
                        PushBinOpTok(BinOperatorType.Gt, ">");
                        continue;

                    case ',':
                        PushTok(TokenType.Comma, ",");
                        continue;

                    case '+' when next is '+':
                        _pos++;
                        PushBinOpTok(BinOperatorType.Concat, "++");
                        continue;
                    case '+':
                        PushBinOpTok(BinOperatorType.Add, "+");
                        continue;

                    case '-':
                        PushBinOpTok(BinOperatorType.Sub, "-");
                        continue;

                    case '*':
                        PushBinOpTok(BinOperatorType.Mul, "*");
                        continue;

                    case '/':
                        PushBinOpTok(BinOperatorType.Div, "/");
                        continue;

                    case '%':
                        PushBinOpTok(BinOperatorType.Mod, "%");
                        continue;

                    case '|':
                        PushTok(TokenType.Pipe, "|");
                        continue;

                    case ':':
                        PushTok(TokenType.Colon, ":");
                        continue;
                    
                    case '#':
                        PushTok(TokenType.Hash, "#");
                        continue;
                    
                    case '!':
                        PushTok(TokenType.Bang, "!");
                        continue;
                }
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
        string value = _value.ToString();
        if (_state != State.String)
        {
            value = value.Trim();
        }
        _tokens.AddLast(new Token(value, type, _line, BinOperatorType.None));

        _value.Clear();
    }

    private void PushTok(TokenType type, string expressValue)
    {
        if (_value.Length != 0)
        {
            PushTokInferType();
        }

        _value.Append(expressValue);
        PushTok(type);
    }

    private void PushBinOpTok(BinOperatorType type, string value)
    {
        if (_value.Length != 0)
        {
            PushTokInferType();
        }

        _tokens.AddLast(new Token(value, TokenType.OpBinary, _line, type));
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

            _ => TokenType.Default
        };

        if (t == TokenType.Default && _state != State.DefaultEscaped)
        {
            if (Double.TryParse(trimmedVal, CultureInfo.InvariantCulture, out _))
            {
                t = TokenType.String;
            }
            else
            {
                t = trimmedVal switch
                {
                    "if"     => TokenType.KwIf,
                    "else"   => TokenType.KwElse,
                    "elseif" => TokenType.KwElseIf,
                    "while"  => TokenType.KwWhile,
                    "for"    => TokenType.KwFor,
                    "repeat" => TokenType.KwRepeat,
                    "until"  => TokenType.KwUntil,
                    "break"  => TokenType.KwBreak,

                    "null"  => TokenType.KwNull,
                    "true"  => TokenType.KwTrue,
                    "false" => TokenType.KwFalse,

                    _ => TokenType.Default
                };
            }
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