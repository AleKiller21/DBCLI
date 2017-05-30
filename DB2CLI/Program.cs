using System;
using SqlParser.SyntaxAnalyser;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser("create database \"C:\\temp\\sample.db\" 4096 mb;".ToLower());
            parser.Parse();
            Console.WriteLine("SUCCESS!!");
        }
    }
}
