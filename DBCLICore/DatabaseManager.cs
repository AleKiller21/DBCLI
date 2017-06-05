using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DBCLICore.Exceptions;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes;
using SqlParser.SyntaxAnalyser.Nodes.ExpressionNodes;
using SqlParser.SyntaxAnalyser.Nodes.Operators;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DeleteNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.DropNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.SelectNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.UpdateNodes;

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
            var entry = GetDirectoryEntry(tableName);
            entry.Available = true;
            Array.Clear(entry.Name, 0, entry.Name.Length);

            var inode = ManagerUtilities.GetInode(entry.Inode);
            var blockSize = Disk.Structures.Super.BlockSize;
            inode.Available = true;
            inode.RecordsAdded = 0;
            Disk.Structures.Super.FreeInodes++;

            var blocks = new List<int>{(int)inode.DataBlockPointer / blockSize, (int)inode.TableInfoBlockPointer / blockSize};
            
            _fileStream.DeleteAllRecords(inode);

            ManagerUtilities.FreeBlocks(blocks);
            FlushDisk(entry, inode);
        }

        public void InsertRecords(InsertNode node)
        {
            var entry = GetDirectoryEntry(node.TargetTable.ToString());

            ManagerUtilities.CheckNewRecordConsistency(entry.Inode, node.Values);
            _fileStream.WriteNewRecord(ManagerUtilities.GetInode(entry.Inode), node.Values);
        }

        public List<string> SelectRecords(SelectNode node)
        {
            var entry = GetDirectoryEntry(node.SourceTable.ToString());

            var inode = ManagerUtilities.GetInode(entry.Inode);
            var records = _fileStream.ReadRecords(inode);
            return ProyectSelection(records, inode.Columns, node.Columns, node.Selection);
        }

        public void UpdateRecords(UpdateNode node)
        {
            var entry = GetDirectoryEntry(node.SourceTable.ToString());
            var inode = ManagerUtilities.GetInode(entry.Inode);
            var records = _fileStream.ReadRecords(inode);

            var selection = node.Selection as UnaryExpressionNode;
            if (selection != null) UpdateRecordsWithSelection(node, inode, records, selection);
            else
            {
                UpdateAllTableRecordsInMemory(node, records, inode);
                _fileStream.UpdateAllRecords(inode, records);
            }
        }

        public void DeleteRecords(DeleteNode node)
        {
            var entry = GetDirectoryEntry(node.SourceTable.ToString());
            var inode = ManagerUtilities.GetInode(entry.Inode);

            var selection = node.Selection as UnaryExpressionNode;
            if (selection == null) _fileStream.DeleteAllRecords(inode);
            else DeleteRecordsWithSelection(node, inode);
        }

        private void DeleteRecordsWithSelection(DeleteNode node, Inode inode)
        {
            var records = _fileStream.ReadRecords(inode);
            var expression = node.Selection as UnaryExpressionNode;
            var selectionColumn = expression.Expression.LeftOperand.ToString();
            var aliveRecords = new List<Record>();
            var columnPos = ManagerUtilities.GetColumnPosition(inode, selectionColumn);

            foreach (var record in records)
            {
                var value = record.Values[columnPos].Value.Evaluate();
                if (value is string) value = ((string)value).Replace("\0", string.Empty);
                if (!expression.Expression.Evaluate(value)) aliveRecords.Add(new Record { Values = record.Values});
            }

            _fileStream.DeleteRecordsWithSelection(inode, aliveRecords);
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

        private void UpdateRecordsWithSelection(UpdateNode node, Inode inode, List<Record> records, UnaryExpressionNode selection)
        {
            var expression = selection.Expression;
            var selectionColumn = expression.LeftOperand.ToString();
            var updatedRecords = new List<UpdatedRecord>();
            var columnPos = ManagerUtilities.GetColumnPosition(inode, selectionColumn);

            for (var i = 0; i < records.Count; i++)
            {
                var value = records[i].Values[columnPos].Value.Evaluate();
                if (value is string) value = ((string)value).Replace("\0", string.Empty);
                if (expression.Evaluate(value)) updatedRecords.Add(new UpdatedRecord { Record = records[i], RecordNumber = i });
            }

            if (updatedRecords.Count == 0) throw new RecordMismatchSelection();
            UpdateRecordsInMemoryWithSelection(node, updatedRecords, inode);

            _fileStream.UpdateRecordsWithSelection(inode, updatedRecords);
        }

        private void UpdateRecordsInMemoryWithSelection(UpdateNode node, List<UpdatedRecord> updatedRecords, Inode inode)
        {
            foreach (var update in node.Updates)
            {
                var columnPos = ManagerUtilities.GetColumnPosition(inode, update.Column.ToString());
                foreach (var record in updatedRecords)
                {
                    record.Record.Values[columnPos].Value = update.Value;
                }
            }
        }

        private void UpdateAllTableRecordsInMemory(UpdateNode node, List<Record> records, Inode inode)
        {
            foreach (var update in node.Updates)
            {
                var columnPos = ManagerUtilities.GetColumnPosition(inode, update.Column.ToString());
                foreach (var record in records)
                {
                    record.Values[columnPos].Value = update.Value;
                }
            }
        }

        private void FlushDisk(DirectoryEntry entry, Inode inode)
        {
            _fileStream.WriteSuperBlock();
            _fileStream.WriteBitmap();
            _fileStream.WriteDirectoryEntry(entry);
            _fileStream.WriteInode(inode);
        }

        private List<string> ProyectAllRecords(List<Record> records, List<ColumnMetadata> columns)
        {
            return FormatProyection(records, columns);
        }

        private List<string> ProyectFilteredColumns(List<Record> records, List<ColumnMetadata> inodeColumns, List<IdNode> filteredColumns)
        {
            var columnPositions = new List<int>();
            foreach (var t in filteredColumns)
            {
                for (var x = 0; x < inodeColumns.Count; x++)
                {
                    if (!new string(inodeColumns[x].Name).Replace("\0", string.Empty).Equals(t.ToString())) continue;

                    columnPositions.Add(x);
                    break;
                }
            }

            var newColumns = columnPositions.Select(pos => inodeColumns[pos]).ToList();
            var newRecords = new List<Record>();
            foreach (var record in records)
            {
                var newRecord = new Record
                {
                    Values = columnPositions.Select(pos => record.Values[pos]).ToList()
                };
                newRecords.Add(newRecord);
            }

            return FormatProyection(newRecords, newColumns);
        }

        private List<string> ProyectSelection(List<Record> records, List<ColumnMetadata> columns, List<IdNode> filteredColumns,
            ConditionalExpressionNode selection)
        {
            var node = selection as UnaryExpressionNode;
            if(node == null) return filteredColumns[0].ToString() == "*" ? ProyectAllRecords(records, columns) : ProyectFilteredColumns(records, columns, filteredColumns);

            var expression = node.Expression;
            var selectionColumn = expression.LeftOperand.ToString();
            var filteredRecords = new List<Record>();
            var columnPos = -1;

            for (var i = 0; i < columns.Count; i++)
            {
                if(!new string(columns[i].Name).Replace("\0", string.Empty).Equals(selectionColumn)) continue;
                columnPos = i;
                break;
            }

            if (columnPos == -1) throw new ColumnNotFoundException();
            foreach (var record in records)
            {
                var value = record.Values[columnPos].Value.Evaluate();
                if (value is string) value = ((string) value).Replace("\0", string.Empty);
                if(expression.Evaluate(value)) filteredRecords.Add(record);
            }

            return filteredColumns[0].ToString() == "*" ? ProyectAllRecords(filteredRecords, columns) : ProyectFilteredColumns(filteredRecords, columns, filteredColumns);
        }

        private List<string> FormatProyection(List<Record> records, List<ColumnMetadata> columns)
        {
            const int width = 20;
            var prints = new List<string>
            {
                "\n",
                string.Join(string.Empty, columns.Select(column => $"{new string(column.Name).Replace("\0", string.Empty),width}|").ToList()),
                "____________________________________________________________________________________________________________________"
            };

            foreach (var record in records)
            {
                prints.Add(string.Join(string.Empty, record.Values.Select(value => $"{value.Value.Evaluate().ToString().Replace("\0", string.Empty),width}|").ToList()));
            }

            return prints;
        }

        private DirectoryEntry GetDirectoryEntry(string tableName)
        {
            var entry = ManagerUtilities.GetDirectoryEntry(tableName);
            if (entry == null) throw new TableNotFoundException();

            return entry;
        }
    }
}
