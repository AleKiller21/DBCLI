using System;
using DBCLICore;
using SqlParser.SyntaxAnalyser;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var query = "drop database jack;";
            var parser = new Parser(query.ToLower());
            var fileDatabase = new FileDatabase();
            fileDatabase.DropDatabase(parser.Parse() as DropDatabaseNode);
        }
    }
}
