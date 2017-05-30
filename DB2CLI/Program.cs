using System;
using SqlParser.SyntaxAnalyser;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser("update employee set salary = 5600.25 where name = \"Alejandro\" and id = 5;".ToLower());
            parser.Parse();
            Console.WriteLine("SUCCESS!!");
        }
    }
}
