using System;
using SqlParser.SyntaxAnalyser;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser("update sample set age = 25 where id = 5 and name = \"jack\";".ToLower());
            parser.Parse();
            Console.WriteLine("SUCCESS!!");
        }
    }
}
