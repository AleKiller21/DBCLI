using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DBCLICore.Exceptions;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class DatabaseManager
    {
        private bool _connection;
        private readonly FileDatabaseWriter _writer;
        private readonly FileDatabaseReader _reader;

        public DatabaseManager()
        {
            _connection = false;
            _writer = new FileDatabaseWriter();
            _reader = new FileDatabaseReader();
        }

        public void CreateDatabase(string name, long size, UnitSize unit)
        {
            size = ManagerUtilities.ConvertToBytes(size, unit);

            if (!ManagerUtilities.CheckIfSizeDivisibleByTwo(size)) throw new NotDivisibleByTwoException(size);
            _writer.CreateDatabase(ManagerUtilities.GetQualifiedName(name), size, 512);
        }

        public void ConnectDatabase(string name)
        {
            if (_connection) throw new SessionActiveException($"You already established a session to {name} database");

            Disk.CurrentDatabase = ManagerUtilities.GetQualifiedName(name);
            _reader.ConnectDatabase(Disk.CurrentDatabase);
            _connection = true;
        }

        public void DisconnectDatabase()
        {
            if (!_connection) throw new SessionNotCreatedException();

            Disk.Structures.Super = null;
            Disk.Structures.Inodes = null;
            Disk.Structures.BitMap = null;
            Disk.Structures.Directory = null;
            Disk.Structures = null;
            Disk.CurrentDatabase = "";
            _connection = false;
        }

        public void DropDatabase(string name)
        {
            if (_connection) throw new SessionActiveException();
            if (!File.Exists(ManagerUtilities.GetQualifiedName(name))) throw new FileNotFoundException("No such database exists.");

            File.Delete(ManagerUtilities.GetQualifiedName(name));
        }

        public void CreateTable(CreateTableNode table)
        {
            if (!_connection) throw new SessionNotCreatedException();
            if(!ManagerUtilities.CheckFreeSpace()) throw new NotEnoughFreeInodesException();
            if (table.Columns.Count > Disk.Structures.Super.BlockSize / ColumnMetadata.Size())
            {
                Console.WriteLine("Too many columns!");
                return;
            }

            var blocks = ManagerUtilities.GetBlocksFromBitmap(2);
            var inode = ManagerUtilities.SetUpInode(blocks, table.Columns);
            var directoryEntry = ManagerUtilities.SetUpDirectoryEntry(table.Name.ToString(), inode);

            _writer.WriteSuperBlock();
            _writer.WriteBitmap();
            _writer.WriteDirectoryEntry(directoryEntry);
            _writer.WriteInode(inode);
        }

        public List<string> ShowTables()
        {
            if(!_connection) throw new SessionNotCreatedException();

            var entries = Disk.Structures.Directory.Where(entry => !entry.Available).ToList();
            var prints = new List<string>();

            foreach (var entry in entries)
            {
                var inode = Disk.Structures.Inodes.First(ind => ind.Number == entry.Inode);
                var recordSize = inode.RecordSize;
                var totaRecords = inode.RecordsAdded;

                foreach (var column in inode.Columns)
                {
                    prints.Add(
                        $"|{new string(entry.Name).Replace("\0", string.Empty),20}|{new string(column.Name).Replace("\0", string.Empty),20}|{column.Type,20}|{column.Type.Size,20}|{recordSize,20}|{totaRecords,20}|");
                }
            }

            return prints;
        }
    }
}
