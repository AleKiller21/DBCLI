using System;
using DBCLICore;
using SqlParser;
using SqlParser.SyntaxAnalyser;
using SqlParser.SyntaxAnalyser.Exceptions;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DatabaseConnectionNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

namespace DB2CLI
{
    public class CLI
    {
        private FileDatabase _database;

        public CLI()
        {
            _database = new FileDatabase();
        }

        public void Start()
        {
            var query = "";
            while (!query.Equals("quit"))
            {
                Console.Write("db2cli>");
                query = Console.ReadLine();
                if(query.Equals("") || query.Equals("quit")) continue;

                StatementNode statement;
                try
                {
                    var parser = new Parser(query.ToLower());
                    statement = parser.Parse();
                }
                catch (ParserException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (LexerException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                EvaluateCommand(statement);
            }
        }

        private void EvaluateCommand(StatementNode node)
        {
            if(node is CreateDatabaseNode) _database.CreateDatabase((CreateDatabaseNode) node);
            else if(node is ConnectDatabaseNode) _database.ConnectDatabase((ConnectionNode) node);
            else if(node is DropDatabaseNode) _database.DropDatabase((DropDatabaseNode) node);
            else if(node is DisconnectDatabaseNode) _database.DisconnectDatabase();
            else if(node is CreateTableNode) _database.CreateTable((CreateTableNode)node);
        }
    }
}
