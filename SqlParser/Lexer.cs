using System.Collections.Generic;
using System.Text;

namespace SqlParser
{
    public class Lexer
    {
        private Dictionary<string, TokenType> _reservedWords;
        private Dictionary<string, TokenType> _operators;
        private Dictionary<string, TokenType> _symbols;
        private readonly string _command;
        private char _currentSymbol;
        private int _row;
        private int _col;
        private int _iterator;

        public Lexer(string command)
        {
            _row = 1;
            _col = 1;
            _iterator = 0;
            _command = command;
            _currentSymbol = _command[_iterator++];
            InitReservedWordsDictionary();
            InitOperatorsDictionary();
            InitSymbolsDictionary();
        }

        public Token GetToken()
        {
            while (_currentSymbol != '$')
            {
                while (char.IsWhiteSpace(_currentSymbol))
                {
                    if (_currentSymbol == '\n')
                    {
                        _row++;
                        _col = 0;
                    }
                    _currentSymbol = GetNextSymbol();
                }

                if (char.IsDigit(_currentSymbol)) return GetNumToken();
                if (char.IsLetter(_currentSymbol)) return GetIdToken();
                if (_currentSymbol == '"') return GetStringToken();
                try
                {
                    var lexeme = new StringBuilder(_currentSymbol + "");
                    var row = _row;
                    var col = _col;
                    var token = new Token(lexeme.ToString(), _symbols[lexeme.ToString()], row, col);
                    _currentSymbol = GetNextSymbol();
                    return token;
                }
                catch (KeyNotFoundException e)
                {
                }
                try
                {
                    var lexeme = new StringBuilder(_currentSymbol + "");
                    var row = _row;
                    var col = _col;
                    var token = new Token(lexeme.ToString(), _operators[lexeme.ToString()], row, col);
                    _currentSymbol = GetNextSymbol();
                    return token;
                }
                catch (KeyNotFoundException e)
                {
                }

                throw new LexerException($"Invalid token at row {_row} column {_col}");
            }

            return new Token("$", TokenType.Eof, _row, _col);
        }

        private Token GetStringToken()
        {
            var lexeme = new StringBuilder(_currentSymbol + "");
            var row = _row;
            var col = _col;

            _currentSymbol = GetNextSymbol();
            if (_currentSymbol == '"')
                throw new LexerException($"Empty strings are not allowed. Row {row} column {col}.");

            while (_currentSymbol != '"')
            {
                lexeme.Append(_currentSymbol);
                _currentSymbol = GetNextSymbol();
            }

            lexeme.Append(_currentSymbol);
            _currentSymbol = GetNextSymbol();
            return new Token(lexeme.ToString(), TokenType.LiteralString, row, col);
        }

        private Token GetIdToken()
        {
            var lexeme = new StringBuilder(_currentSymbol + "");
            var row = _row;
            var col = _col;

            _currentSymbol = GetNextSymbol();
            while (char.IsLetterOrDigit(_currentSymbol) || _currentSymbol == '_')
            {
                lexeme.Append(_currentSymbol);
                _currentSymbol = GetNextSymbol();
            }

            try
            {
                return new Token(lexeme.ToString(), _reservedWords[lexeme.ToString()], row, col);
            }
            catch (KeyNotFoundException e)
            {
                return new Token(lexeme.ToString(), TokenType.Id, row, col);
            }
        }

        private Token GetNumToken()
        {
            var lexeme = new StringBuilder(_currentSymbol + "");
            var row = _row;
            var col = _col;

            _currentSymbol = GetNextSymbol();
            while (char.IsDigit(_currentSymbol))
            {
                lexeme.Append(_currentSymbol);
                _currentSymbol = GetNextSymbol();
            }

            if (_currentSymbol == '.') return GetDoubleToken(lexeme, row, col);

            return new Token(lexeme.ToString(), TokenType.LiteralInt, row, col);
        }

        private Token GetDoubleToken(StringBuilder lexeme, int row, int col)
        {
            lexeme.Append(_currentSymbol);
            _currentSymbol = GetNextSymbol();

            if(!char.IsDigit(_currentSymbol)) throw new LexerException($"Unrecognized token found at row {row} column {col}. Double token expected.");

            while (char.IsDigit(_currentSymbol))
            {
                lexeme.Append(_currentSymbol);
                _currentSymbol = GetNextSymbol();
            }

            return new Token(lexeme.ToString(), TokenType.LiteralDouble, row, col);
        }

        private char GetNextSymbol()
        {
            _col++;
            return _iterator >= _command.Length ? '$' : _command[_iterator++];
        }

        private void InitSymbolsDictionary()
        {
            _symbols = new Dictionary<string, TokenType>
            {
                {"(", TokenType.ParenthesisOpen},
                {")", TokenType.ParenthesisClose},
                {";", TokenType.EndStatement},
                {",", TokenType.Comma}
            };

        }

        private void InitOperatorsDictionary()
        {
            _operators = new Dictionary<string, TokenType>
            {
                {"*", TokenType.OpAll},
                {"=", TokenType.OpEqual},
                {"!=", TokenType.OpNotEqual },
                {">", TokenType.OpGreaterThan},
                {"<", TokenType.OpLessThan },
                {">=", TokenType.OpGreaterThanOrEqual },
                {"<=", TokenType.OpLessThanOrEqual }
            };

        }

        private void InitReservedWordsDictionary()
        {
            _reservedWords = new Dictionary<string, TokenType>
            {
                {"connect", TokenType.RwConnect},
                {"disconnect", TokenType.RwDisconnect},
                {"create", TokenType.RwCreate},
                {"drop", TokenType.RwDrop},
                {"insert", TokenType.RwInsert},
                {"select", TokenType.RwSelect},
                {"update", TokenType.RwUpdate},
                {"delete", TokenType.RwDelete},
                {"into", TokenType.RwInto},
                {"values", TokenType.RwValues},
                {"from", TokenType.RwFrom},
                {"set", TokenType.RwSet},
                {"where", TokenType.RwWhere},
                {"database", TokenType.RwDatabase},
                {"table", TokenType.RwDatabase},
                {"and", TokenType.RwAnd},
                {"or", TokenType.RwOr},
                {"mb", TokenType.RwMb},
                {"gb", TokenType.RwGb},
                {"int", TokenType.RwInt},
                {"double", TokenType.RwDouble},
                {"char", TokenType.RwChar}
            };

        }
    }
}
