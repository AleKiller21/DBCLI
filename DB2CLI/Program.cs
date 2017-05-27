﻿using System;
using SqlParser.SyntaxAnalyser;

namespace DB2CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(";".ToLower());
            parser.Parse();
            Console.WriteLine("SUCCESS!!");
        }
    }
}
