using System.Collections.Generic;
using SqlParser.SyntaxAnalyser.Exceptions;
using SqlParser.SyntaxAnalyser.Nodes;
using SqlParser.SyntaxAnalyser.Nodes.ExpressionNodes;
using SqlParser.SyntaxAnalyser.Nodes.LiteralNodes;
using SqlParser.SyntaxAnalyser.Nodes.Operators;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DatabaseConnectionNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DeleteNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.SelectNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.UpdateNodes;
using SqlParser.SyntaxAnalyser.Nodes.TypeNodes;

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

        public StatementNode Parse()
        {
            return Statement();
        }

        private StatementNode Statement()
        {
            var statement = StatementName();

            if (!CheckToken(TokenType.EndStatement))
                throw new EndOfStatementException(GetTokenRow(), GetTokenColumn());

            NextToken();
            return statement;
        }

        private StatementNode StatementName()
        {
            if (CheckToken(TokenType.RwConnect)) return ConnectDatabase();
            if (CheckToken(TokenType.RwDisconnect)) return DisconnectDatabase();
            if (CheckToken(TokenType.RwCreate)) return CreateObject();
            if (CheckToken(TokenType.RwDrop)) return DropObject();
            if (CheckToken(TokenType.RwInsert)) return InsertTable();
            if (CheckToken(TokenType.RwSelect)) return SelectTable();
            if (CheckToken(TokenType.RwUpdate)) return UpdateTable();
            if (CheckToken(TokenType.RwDelete)) return DeleteTable();

            throw new ParserException($"Unexpected token encountered at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private ConnectionNode DisconnectDatabase()
        {
            if(!CheckToken(TokenType.RwDisconnect))
                throw new ParserException($"'disconnect' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");

            NextToken();

            if (!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var databaseName = new IdNode(_currenToken.Lexeme);
            NextToken();

            return new DisconnectDatabaseNode{DatabaseName = databaseName};
        }

        private ConnectionNode ConnectDatabase()
        {
            if (!CheckToken(TokenType.RwConnect))
                throw new ParserException($"'connect' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");

            NextToken();

            if (!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var databaseName = new IdNode(_currenToken.Lexeme);
            NextToken();

            return new ConnectDatabaseNode() { DatabaseName = databaseName };
        }

        private CreateObjectNode CreateObject()
        {
            if (!CheckToken(TokenType.RwCreate))
                throw new CreateKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            return CreateObjectPrime();
        }

        private CreateObjectNode CreateObjectPrime()
        {
            if (CheckToken(TokenType.RwDatabase))
            {
                NextToken();
                return CreateDatabase();
            }
            if (CheckToken(TokenType.RwTable))
            {
                NextToken();
                return CreateTable();
            }

            throw new ParserException($"'database' or 'table' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private DropObjectNode DropObject()
        {
            if (!CheckToken(TokenType.RwDrop))
                throw new DropKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            return DatabaseOrTable();
        }

        private DropObjectNode DatabaseOrTable()
        {
            if(CheckToken(TokenType.RwDatabase))
            {
                NextToken();
                DropDatabaseNode databaseNode = new DropDatabaseNode();
                DatabaseStringOrIdentifier(databaseNode);
                return databaseNode;

            }
            if (CheckToken(TokenType.RwTable))
            {
                NextToken();
                if(!CheckToken(TokenType.Id))
                    throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

                var tableName = new IdNode(_currenToken.Lexeme);
                NextToken();

                return new DropTableNode{Name = tableName};
            }

            throw new ParserException($"'database' or 'table' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private void DatabaseStringOrIdentifier(DropDatabaseNode databaseNode)
        {
            if (CheckToken(TokenType.LiteralString))
            {
                databaseNode.Path = new StringNode(_currenToken.Lexeme);
                NextToken();
            }

            else if (CheckToken(TokenType.Id))
            {
                databaseNode.Name = new IdNode(_currenToken.Lexeme);
                NextToken();
            }

            else throw new ParserException($"string or id token expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private LiteralNode Literal()
        {
            if (CheckToken(TokenType.LiteralString))
            {
                var value = new StringNode(_currenToken.Lexeme);
                NextToken();
                return value;
            }
            if (CheckToken(TokenType.LiteralInt) || CheckToken(TokenType.LiteralDouble)) return Number();
            throw new ParserException($"numeric or string literal expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private LiteralNode Number()
        {
            if (CheckToken(TokenType.LiteralInt))
            {
                var value = new IntNode(int.Parse(_currenToken.Lexeme));
                NextToken();
                return value;
            }
            if (CheckToken(TokenType.LiteralDouble))
            {
                var value = new DoubleNode(double.Parse(_currenToken.Lexeme));
                NextToken();
                return value;
            }

            throw new ParserException($"int or double literal expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private CreateDatabaseNode CreateDatabase()
        {
            if (CheckToken(TokenType.Id))
            {
                var databaseName = new IdNode(_currenToken.Lexeme);

                NextToken();
                if (!CheckToken(TokenType.LiteralInt))
                    throw new IntLiteralExpectedException(GetTokenRow(), GetTokenColumn());

                var size = int.Parse(_currenToken.Lexeme);

                NextToken();
                var unit = Size();

                return new CreateDatabaseNode(databaseName, unit, size);
            }
            if (CheckToken(TokenType.LiteralString))
            {
                var databaseName = new StringNode(_currenToken.Lexeme);

                NextToken();
                if (!CheckToken(TokenType.LiteralInt))
                    throw new IntLiteralExpectedException(GetTokenRow(), GetTokenColumn());

                var size = int.Parse(_currenToken.Lexeme);

                NextToken();
                var unit = Size();

                return new CreateDatabaseNode(databaseName, unit, size);
            }

            throw new ParserException($"string or id token expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private UnitSize Size()
        {
            if(CheckToken(TokenType.RwMb))
            {
                NextToken();
                return UnitSize.Mb;
            }
            if (CheckToken(TokenType.RwGb))
            {
                NextToken();
                return UnitSize.Gb;
            }

            throw new ParserException($"'mb' or 'gb' size keywords expected at row {GetTokenColumn()} column {GetTokenColumn()}.");
        }

        private CreateTableNode CreateTable()
        {
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var tableName = new IdNode(_currenToken.Lexeme);

            NextToken();
            if (!CheckToken(TokenType.ParenthesisOpen))
                throw new ParenthesisOpenExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            var columns = ColumnCreationList();
            if (!CheckToken(TokenType.ParenthesisClose))
                throw new ParenthesisCloseExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();

            return new CreateTableNode{Name = tableName, Columns = columns};
        }

        private List<ColumnNode> ColumnCreationList()
        {
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var columnName = new IdNode(_currenToken.Lexeme);

            NextToken();
            var type = Type();
            var column = new ColumnNode{Name = columnName, Type = type};
            var columnList = ColumnCreationListPrime();

            columnList.Insert(0, column);
            return columnList;
        }

        private List<ColumnNode> ColumnCreationListPrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                if(!CheckToken(TokenType.Id))
                    throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

                var columnName = new IdNode(_currenToken.Lexeme);

                NextToken();
                var type = Type();
                var column = new ColumnNode{Name = columnName, Type = type};
                var columnList = ColumnCreationListPrime();

                columnList.Insert(0, column);
                return columnList;
            }
            
            return new List<ColumnNode>();
        }

        private TypeNode Type()
        {
            if (CheckToken(TokenType.RwInt))
            {
                NextToken();
                return new IntTypeNode();
            }

            if (CheckToken(TokenType.RwDouble))
            {
                NextToken();
                return new DoubleTypeNode();
            }

            if (CheckToken(TokenType.RwChar))
            {
                NextToken();
                if(!CheckToken(TokenType.ParenthesisOpen))
                    throw new ParenthesisOpenExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                if(!CheckToken(TokenType.LiteralInt))
                    throw new IntLiteralExpectedException(GetTokenRow(), GetTokenColumn());

                var size = int.Parse(_currenToken.Lexeme);

                NextToken();
                if(!CheckToken(TokenType.ParenthesisClose))
                    throw new ParenthesisCloseExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                return new StringTypeNode{Size = size};
            }

            throw new ParserException($"'int', 'double', or 'char' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private InsertNode InsertTable()
        {
            if (!CheckToken(TokenType.RwInsert))
                throw new InsertKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if (!CheckToken(TokenType.RwInto))
                throw new IntoKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var targetTable = new IdNode(_currenToken.Lexeme);

            NextToken();
            if (!CheckToken(TokenType.RwValues))
                throw new ValuesKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.ParenthesisOpen))
                throw new ParenthesisOpenExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            var values = ValueList();
            if(!CheckToken(TokenType.ParenthesisClose))
                throw new ParenthesisCloseExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();

            return new InsertNode{TargetTable = targetTable, Values = values};
        }

        private List<ValueNode> ValueList()
        {
            var value = new ValueNode { Value = Literal() };
            var valueList = ValueListPrime();

            valueList.Insert(0, value);
            return valueList;
        }

        private List<ValueNode> ValueListPrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                var value = new ValueNode{ Value = Literal() };
                var valueList = ValueListPrime();

                valueList.Insert(0, value);
                return valueList;
            }

            return new List<ValueNode>();
        }

        private SelectNode SelectTable()
        {
            if (!CheckToken(TokenType.RwSelect))
                throw new SelectKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            var columns = ColumnList();
            if (!CheckToken(TokenType.RwFrom))
                throw new FromKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var sourceTable = new IdNode(_currenToken.Lexeme);
            NextToken();

            return new SelectNode{Columns = columns, SourceTable = sourceTable, Selection = OptionalSelection() };
        }

        private List<IdNode> ColumnList()
        {
            if (CheckToken(TokenType.Id))
            {
                var column = new IdNode(_currenToken.Lexeme);
                NextToken();
                var columnList = ColumnListPrime();

                columnList.Insert(0, column);
                return columnList;
            }

            if (CheckToken(TokenType.OpAll))
            {
                NextToken();
                return new List<IdNode> { new IdNode("*") };
            }

            throw new ParserException($"Column name or '*' token expected at row {GetTokenRow()}, {GetTokenColumn()}.");
        }

        private List<IdNode> ColumnListPrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                if(!CheckToken(TokenType.Id))
                    throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

                var column = new IdNode(_currenToken.Lexeme);
                NextToken();
                var columnList = ColumnListPrime();

                columnList.Insert(0, column);
                return columnList;
            }
            
            return new List<IdNode>();
        }

        private ConditionalExpressionNode OptionalSelection()
        {
            if (CheckToken(TokenType.RwWhere))
            {
                NextToken();
                return OrExpression();
            }

            return null;
        }

        private ConditionalExpressionNode OrExpression()
        {
            return OrExpressionPrime(AndExpression());
        }

        private ConditionalExpressionNode OrExpressionPrime(ConditionalExpressionNode leftOperand)
        {
            if (CheckToken(TokenType.RwOr))
            {
                var Operator = OrOperator();
                Operator.LeftOperand = leftOperand;
                Operator.RightOperand = AndExpression();

                return OrExpressionPrime(Operator);
            }

            return leftOperand;
        }

        private ConditionalExpressionNode AndExpression()
        {
            return AndExpressionPrime(SelectionPredicate());
        }

        private ConditionalExpressionNode AndExpressionPrime(ConditionalExpressionNode leftOperand)
        {
            if (CheckToken(TokenType.RwAnd))
            {
                var Operator = AndOperator();
                Operator.LeftOperand = leftOperand;
                Operator.RightOperand = SelectionPredicate();

                return AndExpressionPrime(Operator);
            }

            return leftOperand;
        }

        private UnaryExpressionNode SelectionPredicate()
        {
            if (!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var identifier = new IdNode(_currenToken.Lexeme);

            NextToken();
            var conditionalNode = SelectionOperators();

            conditionalNode.LeftOperand = identifier;
            conditionalNode.RightOperand = Literal();

            return new UnaryExpressionNode{Expression = conditionalNode};
        }

        private AndExpressionNode AndOperator()
        {
            if (CheckToken(TokenType.RwAnd))
            {
                NextToken();
                return new AndExpressionNode();
            }

            throw new ParserException($"'and' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private OrExpressionNode OrOperator()
        {
            if (CheckToken(TokenType.RwOr))
            {
                NextToken();
                return new OrExpressionNode();
            }

            throw new ParserException($"'or' keyword expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private ConditionalNode SelectionOperators()
        {
            if (IsSelectionOperator())
            {
                ConditionalNode Operator;

                if(CheckToken(TokenType.OpEqual)) Operator = new EqualOperatorNode();
                else if(CheckToken(TokenType.OpNotEqual)) Operator = new NotEqualOperatorNode();
                else if (CheckToken(TokenType.OpGreaterThan)) Operator = new GreaterThanOperatorNode();
                else if (CheckToken(TokenType.OpLessThan)) Operator = new LessThanOperatorNode();
                else if (CheckToken(TokenType.OpGreaterThanOrEqual)) Operator = new GreaterThanOrEqualOperatorNode();
                else Operator = new LessThanOrEqualOperatorNode();

                NextToken();
                return Operator;
            }

            throw new ParserException($"'=', '!=', '>', '<', '>=', or '<=' operator expected at row {GetTokenRow()} column {GetTokenColumn()}.");
        }

        private UpdateNode UpdateTable()
        {
            if(!CheckToken(TokenType.RwUpdate))
                throw new UpdateKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var sourceTable = new IdNode(_currenToken.Lexeme);
            NextToken();
            var updates = SetUpdate();
            var selection = OptionalSelection();

            return new UpdateNode{SourceTable = sourceTable, Updates = updates, Selection = selection};
        }

        private List<SetUpdateNode> SetUpdate()
        {
            if (!CheckToken(TokenType.RwSet))
                throw new SetKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());
            
            var column = new IdNode(_currenToken.Lexeme);
            NextToken();
            if (!CheckToken(TokenType.OpEqual))
                throw new EqualOperatorExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            var value = Literal();
            var update = new SetUpdateNode{Column = column, Value = value};
            var updates = SetUpdatePrime();
            updates.Insert(0, update);

            return updates;
        }

        private List<SetUpdateNode> SetUpdatePrime()
        {
            if (CheckToken(TokenType.Comma))
            {
                NextToken();
                if(!CheckToken(TokenType.Id))
                    throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

                var column = new IdNode(_currenToken.Lexeme);
                NextToken();
                if(!CheckToken(TokenType.OpEqual))
                    throw new EqualOperatorExpectedException(GetTokenRow(), GetTokenColumn());

                NextToken();
                var value = Literal();
                var update = new SetUpdateNode{Column = column, Value = value};
                var updates = SetUpdatePrime();
                updates.Insert(0, update);

                return updates;
            }

            return new List<SetUpdateNode>();
        }

        private DeleteNode DeleteTable()
        {
            if (!CheckToken(TokenType.RwDelete))
                throw new DeleteKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.RwFrom))
                throw new FromKeywordExpectedException(GetTokenRow(), GetTokenColumn());

            NextToken();
            if(!CheckToken(TokenType.Id))
                throw new IdExpectedException(GetTokenRow(), GetTokenColumn());

            var sourceTable = new IdNode(_currenToken.Lexeme);
            NextToken();
            ConditionalExpressionNode selection = OptionalSelection();

            return new DeleteNode{SourceTable = sourceTable, Selection = selection};
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
