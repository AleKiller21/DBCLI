using System;
using System.Runtime.InteropServices;
using DBCLICore;
using SqlParser.SyntaxAnalyser;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DatabaseConnectionNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            //var query = "connect sample;";
            //var parser = new Parser(query.ToLower());
            //var fileDatabase = new FileDatabase();
            //fileDatabase.ConnectDatabase(parser.Parse() as ConnectionNode);
        }
    }
}
