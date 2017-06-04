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
            //var buffer = BitConverter.GetBytes(45.8);
            //Console.WriteLine(BitConverter.ToDouble(buffer, 0));
            //Console.WriteLine(BitConverter.IsLittleEndian ? "Little" : " Big");
            //foreach (var c in "hello")
            //{
            //    Console.WriteLine((byte)c);
            //}
        }
    }
}
