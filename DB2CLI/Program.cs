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
            var cli = new CLI();
            cli.Start();
            //var x = UInt32.MaxValue << 32;
            //Console.WriteLine(x);
        }
    }
}
