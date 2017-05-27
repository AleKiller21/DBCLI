using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SqlParser.SyntaxAnalyser.Exceptions;

namespace SqlParser.SyntaxAnalyser
{
    public class Parser
    {
        private Lexer _lex;
        private Token _currenToken;

        public Parser(string query)
        {
            _lex = new Lexer(query);
            _currenToken = _lex.GetToken();
        }

        public void Statement()
        {
            StatementName();
            if (!CheckToken(TokenType.EndStatement))
                throw new EndOfStatementException(GetTokenRow(), GetTokenColumn());

            NextToken();
        }

        private void StatementName()
        {
            if (CheckToken(TokenType.RwCreate)) CreateObject();
            else if (CheckToken(TokenType.RwDrop)) DropObject();
            else if (CheckToken(TokenType.RwInsert)) InsertTable();
            else if (CheckToken(TokenType.RwSelect)) SelectTable();
            else if (CheckToken(TokenType.RwUpdate)) UpdateTable();
            else if (CheckToken(TokenType.RwDelete)) DeleteTable();
            else throw new ParserException($"Unexpected token encountered at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void CreateObject()
        {
            if (!CheckToken(TokenType.RwCreate))
                throw new CreateKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            CreateObjectPrime();
        }

        private void CreateObjectPrime()
        {
            if (CheckToken(TokenType.RwDatabase))
            {
                NextToken();
                CreateDatabase();
            }
            else if (CheckToken(TokenType.RwTable))
            {
                NextToken();
                CreateTable();
            }
            else throw new ParserException($"'database' or 'table' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void DropObject()
        {
            if (!CheckToken(TokenType.RwDrop))
                throw new DropKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            DatabaseOrTable();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
        }

        private void DatabaseOrTable()
        {
            if(CheckToken(TokenType.RwDatabase) || CheckToken(TokenType.RwTable)) NextToken();
            else throw new ParserException($"'database' or 'table' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void Literal()
        {
            if(CheckToken(TokenType.LiteralString)) NextToken();
            else if (CheckToken(TokenType.LiteralInt) || CheckToken(TokenType.LiteralDouble)) Number();
            else throw new ParserException($"numeric or string literal expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void Number()
        {
            if(CheckToken(TokenType.LiteralInt) || CheckToken(TokenType.LiteralDouble)) NextToken();
            else throw new ParserException($"int or double literal expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void CreateDatabase()
        {
            if (!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if (!CheckToken(TokenType.LiteralInt))
                throw new IntLiteralExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            Size();
        }

        private void Size()
        {
            if(CheckToken(TokenType.RwMb) || CheckToken(TokenType.RwGb)) NextToken();
            else throw new ParserException($"'mb' or 'gb' size keywords expected at row {GetTokenColumn()} column {GetTokenColumn()}.");
        }

        private void CreateTable()
        {
            throw new NotImplementedException();
        }

        private void InsertTable()
        {
            throw new NotImplementedException();
        }

        private void SelectTable()
        {
            throw new NotImplementedException();
        }

        private void UpdateTable()
        {
            throw new NotImplementedException();
        }

        private void DeleteTable()
        {
            throw new NotImplementedException();
        }

        private void NextToken()
        {
            _currenToken = _lex.GetToken();
        }

        private bool CheckToken(TokenType type)
        {
            return _currenToken.Type == type;
        }

        private int GetTokenRow()
        {
            return _currenToken.Row;
        }

        private int GetTokenColumn()
        {
            return _currenToken.Col;
        }
    }
}
