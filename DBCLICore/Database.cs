﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class Database
    {
        private const string Path = "..\\Databases\\";
        private const int BufferSize = 1024;

        public void CreateDatabase(string name, long size, UnitSize unit)
        {
            size = ConvertToBytes(size, unit);

            if (!CheckIfSizeDivisibleByTwo(size))
            {
                Console.WriteLine("Invalid size. Size must be divisible by 2.");
                return;
            }

            var buffer = new byte[BufferSize];

            using (var writer = new BinaryWriter(File.Open(Path + name + ".db", FileMode.CreateNew)))
            {
                for (long i = 0; i < size; i += BufferSize)
                {
                    writer.Write(buffer);
                }
            }

            Console.WriteLine($"{name} database has been created successfully.");
        }

        private long ConvertToBytes(long size, UnitSize unit)
        {
            size *= 1024;
            if (unit == UnitSize.Mb) return size;
            return size * 1024;
        }

        private bool CheckIfSizeDivisibleByTwo(long size)
        {
            return size % 2 == 0;
        }
    }
}
