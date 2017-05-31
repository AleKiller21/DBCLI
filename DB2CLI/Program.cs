using System;
using DBCLICore;
using SqlParser.SyntaxAnalyser;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser("create database jack 50 mb;".ToLower());
            var fileDatabase = new FileDatabase();
            fileDatabase.CreateDatabase(parser.Parse() as CreateDatabaseNode);
        }
    }
}
