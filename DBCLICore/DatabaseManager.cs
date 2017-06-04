using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DBCLICore.Exceptions;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;

namespace DBCLICore
{
    public class DatabaseManager
    {
        private bool _connection;
        private readonly FileDatabaseStream _fileStream;

        public DatabaseManager()
        {
            _connection = false;
            _fileStream = new FileDatabaseStream();
        }

        public void CreateDatabase(string name, long size, UnitSize unit)
        {
            size = ManagerUtilities.ConvertToBytes(size, unit);

            if (!ManagerUtilities.CheckIfSizeDivisibleByTwo(size)) throw new NotDivisibleByTwoException(size);
            _fileStream.CreateDatabase(ManagerUtilities.GetQualifiedName(name), size, 512);
        }

        public void ConnectDatabase(string name)
        {
            if (_connection) throw new SessionActiveException($"You already established a session to {name} database");

            Disk.CurrentDatabase = ManagerUtilities.GetQualifiedName(name);
            _fileStream.ConnectDatabase(Disk.CurrentDatabase);

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

            _fileStream.CloseStream();
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
            if (table.Columns.FirstOrDefault(column => column.Type.Size > 4000) != null)
                throw new ColumnSizeOutOfRangeException();
            if(!ManagerUtilities.CheckFreeSpace()) throw new NotEnoughFreeInodesException();
            if (table.Columns.Count > Disk.Structures.Super.BlockSize / ColumnMetadata.Size())
            {
                Console.WriteLine("Too many columns!");
                return;
            }

            var blocks = ManagerUtilities.GetBlocksFromBitmap(2);
            var inode = ManagerUtilities.SetUpInode(blocks, table.Columns);
            var directoryEntry = ManagerUtilities.SetUpDirectoryEntry(table.Name.ToString(), inode);

            FlushDisk(directoryEntry, inode);
        }

        public void DropTable(string tableName)
        {
            var entry = ManagerUtilities.GetDirectoryEntry(tableName);
            entry.Available = true;
            Array.Clear(entry.Name, 0, entry.Name.Length);

            var inode = ManagerUtilities.GetInode(entry.Inode);
            var blockSize = Disk.Structures.Super.BlockSize;
            inode.Available = true;
            inode.RecordsAdded = 0;
            Disk.Structures.Super.FreeInodes++;

            var blocks = new List<int>{(int)inode.DataBlockPointer / blockSize, (int)inode.TableInfoBlockPointer / blockSize};
            //TODO Obtener los bloques de registros de la tabla
            ManagerUtilities.FreeBlocks(blocks);
            FlushDisk(entry, inode);
        }

        public void InsertRecords(InsertNode node)
        {
            var entry = ManagerUtilities.GetDirectoryEntry(node.TargetTable.ToString());
            if (entry == null) throw new TableNotFoundException();

            ManagerUtilities.CheckNewRecordConsistency(entry.Inode, node.Values);
            _fileStream.WriteNewRecord(ManagerUtilities.GetInode(entry.Inode), node.Values);
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

        public List<string> ShowSuperBlock()
        {
            if(!_connection) throw new SessionNotCreatedException();

            return Disk.Structures.Super.GetType().GetFields()
                .Select(field => $"{field.Name}: {field.GetValue(Disk.Structures.Super)}").ToList();
        }

        private void FlushDisk(DirectoryEntry entry, Inode inode)
        {
            _fileStream.WriteSuperBlock();
            _fileStream.WriteBitmap();
            _fileStream.WriteDirectoryEntry(entry);
            _fileStream.WriteInode(inode);
        }
    }
}
