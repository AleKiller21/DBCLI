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

            //var arr = new byte[4];
            //var x = "hell";
            //for (var i = 0; i < x.Length; i++)
            //{
            //    arr[i] = (byte) x[i];
            //}

            //Console.WriteLine(BitConverter.ToString(arr));
        }
    }
}
