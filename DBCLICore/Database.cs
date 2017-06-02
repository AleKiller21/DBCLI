using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DBCLICore.Exceptions;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class Database
    {
        private const string Path = "..\\Databases\\";
        private string _currentDatabase;
        private readonly FileDatabaseWriter _writer;
        private readonly FileDatabaseReader _reader;

        public Database()
        {
            _writer = new FileDatabaseWriter();
            _reader = new FileDatabaseReader();
        }

        public void CreateDatabase(string name, long size, UnitSize unit)
        {
            size = ConvertToBytes(size, unit);

            if (!CheckIfSizeDivisibleByTwo(size)) throw new NotDivisibleByTwoException(size);
            _writer.CreateDatabase(GetQualifiedName(name), size, 512);
        }

        public FileDatabaseStructures ConnectDatabase(string name)
        {
            _currentDatabase = GetQualifiedName(name);

            var structures = _reader.ConnectDatabase(_currentDatabase);
            return structures;
        }

        //TODO Disconnect method

        public void DropDatabase(string name)
        {
            if (!File.Exists(GetQualifiedName(name))) throw new FileNotFoundException("No such database exists.");
            File.Delete(GetQualifiedName(name));
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
