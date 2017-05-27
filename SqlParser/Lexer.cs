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

            return null;
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

            return new Token(lexeme.ToString(), TokenType.LiteralInt, row, col);
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
            _reservedWords = new Dictionary<string, TokenType>();

            _reservedWords.Add("create", TokenType.RwCreate);
            _reservedWords.Add("drop", TokenType.RwDrop);
            _reservedWords.Add("insert", TokenType.RwInsert);
            _reservedWords.Add("select", TokenType.RwSelect);
            _reservedWords.Add("update", TokenType.RwUpdate);
            _reservedWords.Add("delete", TokenType.RwDelete);
            _reservedWords.Add("into", TokenType.RwInto);
            _reservedWords.Add("values", TokenType.RwValues);
            _reservedWords.Add("from", TokenType.RwFrom);
            _reservedWords.Add("set", TokenType.RwSet);
            _reservedWords.Add("where", TokenType.RwWhere);
            _reservedWords.Add("database", TokenType.RwDatabase);
            _reservedWords.Add("and", TokenType.RwAnd);
            _reservedWords.Add("or", TokenType.RwOr);
            _reservedWords.Add("mb", TokenType.RwMb);
            _reservedWords.Add("gb", TokenType.RwGb);
        }
    }
}
