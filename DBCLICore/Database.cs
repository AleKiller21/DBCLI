using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class Database
    {
        private const string Path = "..\\Databases\\";
        private string _currentDatabase;
        private bool _connection;
        private readonly FileDatabaseWriter _writer;
        private readonly FileDatabaseReader _reader;

        public Database()
        {
            _connection = false;
            _writer = new FileDatabaseWriter();
            _reader = new FileDatabaseReader();
        }

        public void CreateDatabase(string name, long size, UnitSize unit)
        {
            size = ConvertToBytes(size, unit);

            if (!CheckIfSizeDivisibleByTwo(size))
            {
                Console.WriteLine("Invalid size. Must be divisible by 2.");
                return;
            }

            try
            {
                //TODO Send blockSize as user input
                _writer.CreateDatabase(GetQualifiedName(name), size, 512);
                Console.WriteLine($"{name} database has been created successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occurred.\n" + e);
            }
        }

        public FileDatabaseStructures ConnectDatabase(string name)
        {
            try
            {
                _currentDatabase = GetQualifiedName(name);

                var structures = _reader.ConnectDatabase(_currentDatabase);
                //_connection = true;
                return structures;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("No such database exists!");
                return null;
            }
        }

        //TODO Disconnect method

        public void DropDatabase(string name)
        {
            if (_connection)
            {
                Console.WriteLine("You must disconnect first in order to drop the active database.");
                return;
            }

            try
            {
                File.Delete(GetQualifiedName(name));
                Console.WriteLine($"{name} database has been deleted.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private long ConvertToBytes(long size, UnitSize unit)
        {
            size *= 1024;
            if (unit == UnitSize.Mb) return size * 1024;
            return size * 1024 * 1024;
        }

        private bool CheckIfSizeDivisibleByTwo(long size)
        {
            return size % 2 == 0;
        }

        private string GetQualifiedName(string name)
        {
            return Path + name + ".db";
        }
    }
}
