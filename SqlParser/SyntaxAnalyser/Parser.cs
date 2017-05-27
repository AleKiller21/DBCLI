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

        public void Parse()
        {
            Statement();
        }

        private void Statement()
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
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if (!CheckToken(TokenType.ParenthesisOpen))
                throw new ParenthesisOpenExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            ColumnCreationList();
            if (!CheckToken(TokenType.ParenthesisClose))
                throw new ParenthesisCloseExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
        }

        private void ColumnCreationList()
        {
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            Type();
            ColumnCreationListPrime();
        }

        private void ColumnCreationListPrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                if(!CheckToken(TokenType.Id))
                    throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                Type();
                ColumnCreationListPrime();
            }
            else
            {
                //Epsilon
            }
        }

        private void Type()
        {
            if (CheckToken(TokenType.RwInt) || CheckToken(TokenType.RwDouble)) NextToken();
            else if (CheckToken(TokenType.RwChar))
            {
                NextToken();
                if(!CheckToken(TokenType.ParenthesisOpen))
                    throw new ParenthesisOpenExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                if(!CheckToken(TokenType.LiteralInt))
                    throw new IntLiteralExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                if(!CheckToken(TokenType.ParenthesisClose))
                    throw new ParenthesisCloseExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
            }
            else throw new ParserException($"'int', 'double', or 'char' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void InsertTable()
        {
            if (!CheckToken(TokenType.RwInsert))
                throw new InsertKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if (!CheckToken(TokenType.RwInto))
                throw new IntoKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if (!CheckToken(TokenType.RwValues))
                throw new ValuesKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.ParenthesisOpen))
                throw new ParenthesisOpenExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            ValueList();
            if(!CheckToken(TokenType.ParenthesisClose))
                throw new ParenthesisCloseExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
        }

        private void ValueList()
        {
            Literal();
            ValueListPrime();
        }

        private void ValueListPrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                Literal();
                ValueListPrime();
            }
            else
            {
                //Epsilon
            }
        }

        private void SelectTable()
        {
            if (!CheckToken(TokenType.RwSelect))
                throw new SelectKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            ColumnList();
            if (!CheckToken(TokenType.RwFrom))
                throw new FromKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            OptionalSelection();
        }

        private void ColumnList()
        {
            if (CheckToken(TokenType.Id))
            {
                NextToken();
                ColumnListPrime();
            }
            else if(CheckToken(TokenType.OpAll)) NextToken();
            else throw new ParserException($"Column name or '*' token expected at row {GetTokenRow()}, {GetTokenColumn()}.");
        }

        private void ColumnListPrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                if(!CheckToken(TokenType.Id))
                    throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                ColumnListPrime();
            }
            else
            {
                //Epsilon
            }
        }

        private void OptionalSelection()
        {
            if (CheckToken(TokenType.RwWhere))
            {
                NextToken();
                SelectionPredicate();
                SelectionConjunction();
            }
            else
            {
                //Epsilon
            }
        }

        private void SelectionPredicate()
        {
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            SelectionOperators();
            Literal();
        }

        private void SelectionConjunction()
        {
            if (CheckToken(TokenType.RwAnd) || CheckToken(TokenType.RwOr))
            {
                Conjunction();
                SelectionPredicate();
                SelectionConjunction();
            }
            else
            {
                //Epsilon
            }
        }

        private void Conjunction()
        {
            if (CheckToken(TokenType.RwAnd) || CheckToken(TokenType.RwOr)) NextToken();
            else throw new ParserException($"'and' or 'or' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void SelectionOperators()
        {
            if(IsSelectionOperator()) NextToken();
            else throw new ParserException($"'=', '!=', '>', '<', '>=', or '<=' operator expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void UpdateTable()
        {
            if(!CheckToken(TokenType.RwUpdate))
                throw new UpdateKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            SetUpdate();
            OptionalSelection();
        }

        private void SetUpdate()
        {
            if (!CheckToken(TokenType.RwSet))
                throw new SetKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());
            
            NextToken();
            if (!CheckToken(TokenType.OpEqual))
                throw new EqualOperatorExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            Literal();
            SetUpdatePrime();
        }

        private void SetUpdatePrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                if(!CheckToken(TokenType.Id))
                    throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                if(!CheckToken(TokenType.OpEqual))
                    throw new EqualOperatorExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                Literal();
                SetUpdatePrime();
            }
            else
            {
                //Epsilon
            }
        }

        private void DeleteTable()
        {
            if (!CheckToken(TokenType.RwDelete))
                throw new DeleteKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.RwFrom))
                throw new FromKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            OptionalSelection();
        }

        private void NextToken()
        {
            _currenToken = _lex.GetToken();
        }

        private bool CheckToken(TokenType type)
        {
            return _currenToken.Type == type;
        }

        private bool IsSelectionOperator()
        {
            return CheckToken(TokenType.OpEqual) || CheckToken(TokenType.OpNotEqual) ||
                   CheckToken(TokenType.OpGreaterThan) || CheckToken(TokenType.OpLessThan) ||
                   CheckToken(TokenType.OpGreaterThanOrEqual) || CheckToken(TokenType.OpLessThanOrEqual);
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
