using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes;
using SqlParser.SyntaxAnalyser.Nodes.ExpressionNodes;
using SqlParser.SyntaxAnalyser.Nodes.LiteralNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class FileDatabaseStream
    {
        private FileStream _fileStream;
        private string _path;

        public void CreateDatabase(string path, long size, int blockSize)
        {
            const int bufferSize = 1024;
            var buffer = new byte[bufferSize];

            using (_fileStream = new FileStream(path, FileMode.CreateNew))
            {
                for (long i = 0; i < size; i += bufferSize)
                {
                    _fileStream.Write(buffer, 0, bufferSize);
                }

                Disk.Structures = new FileDatabaseStructures { Super = new SuperBlock(size) };

                _fileStream.Seek(0, SeekOrigin.Begin);
                WriteStructuresToDisk();
                Disk.Structures = null;
            }
        }

        private void WriteStructuresToDisk()
        {
            WriteSuperBlockFirstTime();
            WriteBitMapFirstTime();
            WriteDirectory();
            WriteInodeTable();
        }

        private void WriteSuperBlockFirstTime()
        {
            var buffer = Disk.Structures.Super.ToByteArray();
            _fileStream.Write(buffer, 0, buffer.Length);
        }

        private void WriteBitMapFirstTime()
        {
            _fileStream.Seek(Disk.Structures.Super.BitmapBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

            Disk.Structures.BitMap = new byte[Disk.Structures.Super.BitmapSize];
            var blockCounter = 0;
            const byte msb = 128;

            for (var i = 0; i < Disk.Structures.Super.BitmapSize; i++)
            {
                var word = byte.MaxValue;

                for (byte bit = 0; bit < sizeof(byte) * 8; bit++)
                {
                    if (blockCounter == Disk.Structures.Super.FirstDataBlock) break;
                    word ^= (byte) (msb >> bit);
                    blockCounter++;
                }

                Disk.Structures.BitMap[i] = word;
            }

            _fileStream.Write(Disk.Structures.BitMap, 0, Disk.Structures.BitMap.Length);
        }

        private void WriteDirectory()
        {
            _fileStream.Seek(Disk.Structures.Super.DirectoryBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);
            var buffer = new List<byte>();

            for (var i = 0; i < Disk.Structures.Super.TotalInodes; i++)
            {
                var entry = new DirectoryEntry{Number = i};
                buffer.AddRange(entry.ToByteArray());
            }

            var arrayBuffer = buffer.ToArray();
            _fileStream.Write(arrayBuffer, 0, arrayBuffer.Length);
        }

        private void WriteInodeTable()
        {
            _fileStream.Seek(Disk.Structures.Super.InodeTableBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);
            var listBuffer = new List<byte>();

            for (var i = 0; i < Disk.Structures.Super.TotalInodes; i++)
            {
                var inode = new Inode{Available = true, Number = i};
                listBuffer.AddRange(inode.ToByteArray());
            }

            var arrayBuffer = listBuffer.ToArray();
            _fileStream.Write(arrayBuffer, 0, arrayBuffer.Length);
        }

        public void WriteSuperBlock()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            WriteSuperBlockFirstTime();
        }

        public void WriteBitmap()
        {
            _fileStream.Seek(Disk.Structures.Super.BitmapBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);
            _fileStream.Write(Disk.Structures.BitMap, 0, Disk.Structures.BitMap.Length);
        }

        public void WriteInode(Inode inode)
        {
            var offset = Disk.Structures.Super.InodeTableBlock * Disk.Structures.Super.BlockSize +
                         inode.Number * Inode.Size();

            var buffer = inode.ToByteArray();
            _fileStream.Seek(offset, SeekOrigin.Begin);
            _fileStream.Write(buffer, 0, buffer.Length);

            //TODO No siempre es necesario escribir la metadata de las columnas. Solo al crear una nueva tabla, no al ingresar registros.
            _fileStream.Seek((int)inode.TableInfoBlockPointer, SeekOrigin.Begin);
            var columnMetadataListBuffer = new List<byte>();
            foreach (var column in inode.Columns)
            {
                var typeName = new char[ColumnMetadata.TypeNameSize];
                column.Type.ToString().CopyTo(0, typeName, 0, column.Type.ToString().Length);

                columnMetadataListBuffer.AddRange(CharArrayToByteArray(column.Name));
                columnMetadataListBuffer.AddRange(CharArrayToByteArray(typeName));
                columnMetadataListBuffer.AddRange(BitConverter.GetBytes(column.Type.Size));
            }

            var columnMetadataArrayBuffer = columnMetadataListBuffer.ToArray();
            _fileStream.Write(columnMetadataArrayBuffer, 0, columnMetadataArrayBuffer.Length);
        }

        public void WriteDirectoryEntry(DirectoryEntry entry)
        {
            var offset = Disk.Structures.Super.DirectoryBlock * Disk.Structures.Super.BlockSize + entry.Number * DirectoryEntry.Size();
            var buffer = entry.ToByteArray();
            _fileStream.Seek(offset, SeekOrigin.Begin);
            _fileStream.Write(buffer, 0, buffer.Length);
        }

        public void WriteNewRecord(Inode inode, List<ValueNode> values)
        {
            var blockPointer = GetBlockPointerPosition(inode.CurrentInsertBlockBase);
            var buffer = ConvertRecordValuesToByteArray(values, inode.Columns);
            _fileStream.Seek((int)inode.NextRecordToInsertPointer, SeekOrigin.Begin);

            if (inode.NextRecordToInsertPointer + inode.RecordSize <= blockPointer)
            {
                _fileStream.Write(buffer, 0, buffer.Length);
                inode.NextRecordToInsertPointer = (uint)_fileStream.Position;
                inode.RecordsAdded++;
                WriteInode(inode);
            }
            else
            {
                var remainingSpace = blockPointer - inode.NextRecordToInsertPointer;
                var newBlocksCount = (int)Math.Ceiling((double)(inode.RecordSize - remainingSpace) / Disk.Structures.Super.BytesAvailablePerBlock);
                var newBlocks = ManagerUtilities.GetBlocksFromBitmap(newBlocksCount);

                _fileStream.Write(buffer, 0, (int)remainingSpace);

                var iterator = (int)remainingSpace;
                for (var i = 0; i < newBlocksCount; i++)
                {
                    _fileStream.Write(BitConverter.GetBytes((uint)(newBlocks[i] * Disk.Structures.Super.BlockSize)), 0, sizeof(uint));
                    _fileStream.Seek(newBlocks[i] * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

                    var currentBlockBase = newBlocks[i] * Disk.Structures.Super.BlockSize;
                    var recordSizeLeft = inode.RecordSize - iterator;

                    blockPointer = GetBlockPointerPosition((uint) currentBlockBase);
                    if (_fileStream.Position + recordSizeLeft <= blockPointer)
                    {
                        _fileStream.Write(buffer, iterator, (int) recordSizeLeft);
                        inode.NextRecordToInsertPointer = (uint) _fileStream.Position;
                        inode.CurrentInsertBlockBase = (uint) currentBlockBase;
                        inode.RecordsAdded++;
                        break;
                    }

                    remainingSpace = (uint)(blockPointer - _fileStream.Position);
                    _fileStream.Write(buffer, iterator, (int)remainingSpace);
                    iterator += (int)remainingSpace;
                }

                WriteSuperBlock();
                WriteBitmap();
                WriteInode(inode);
            }
        }

        public void ConnectDatabase(string name)
        {
            _fileStream = new FileStream(name, FileMode.Open);
            _path = name;

            Disk.Structures = new FileDatabaseStructures { Super = ReadSuperBlock() };
            Disk.Structures.BitMap = ReadBitmap();
            Disk.Structures.Directory = ReadDirectory();
            Disk.Structures.Inodes = ReadInodeTable();
        }

        public List<Record> ReadRecords(Inode inode)
        {
            _fileStream.Seek(inode.DataBlockPointer, SeekOrigin.Begin);
            var bufferList = new List<byte>();

            while (_fileStream.Position != inode.CurrentInsertBlockBase)
            {
                var buffer = new byte[Disk.Structures.Super.BytesAvailablePerBlock];
                var pointer = new byte[sizeof(uint)];
                _fileStream.Read(buffer, 0, buffer.Length);
                _fileStream.Read(pointer, 0, pointer.Length);
                bufferList.AddRange(buffer);
                _fileStream.Seek(BitConverter.ToUInt32(pointer, 0), SeekOrigin.Begin);
            }

            var difference = inode.NextRecordToInsertPointer - _fileStream.Position;
            var finalBuffer = new byte[difference];
            _fileStream.Read(finalBuffer, 0, (int)difference);
            bufferList.AddRange(finalBuffer);

            return ConvertByteArrayToRecords(bufferList.ToArray(), inode.Columns);
        }

        public void DeleteAllRecords(Inode inode)
        {
            _fileStream.Seek(inode.DataBlockPointer, SeekOrigin.Begin);
            while (_fileStream.Position != inode.CurrentInsertBlockBase)
            {
                if (_fileStream.Position != inode.DataBlockPointer)
                {
                    var blockNumber = _fileStream.Position / Disk.Structures.Super.BlockSize;
                    ManagerUtilities.FreeBlocks(new List<int> { (int)blockNumber });
                }

                var pointer = new byte[sizeof(uint)];
                _fileStream.Seek(Disk.Structures.Super.BytesAvailablePerBlock, SeekOrigin.Current);
                _fileStream.Read(pointer, 0, pointer.Length);
                _fileStream.Seek(BitConverter.ToUInt32(pointer, 0), SeekOrigin.Begin);
            }

            if (_fileStream.Position != inode.DataBlockPointer)
            {
                var blockNumber = _fileStream.Position / Disk.Structures.Super.BlockSize;
                ManagerUtilities.FreeBlocks(new List<int> { (int)blockNumber });
            }

            inode.NextRecordToInsertPointer = inode.DataBlockPointer;
            inode.CurrentInsertBlockBase = inode.DataBlockPointer;
            WriteInode(inode);
        }

        public void UpdateAllRecords(Inode inode, List<Record> records)
        {
            var bufferList = new List<byte>();
            foreach (var record in records)
            {
                bufferList.AddRange(ConvertRecordValuesToByteArray(record.Values, inode.Columns));
            }

            var buffer = bufferList.ToArray();
            var iterator = 0;
            _fileStream.Seek(inode.DataBlockPointer, SeekOrigin.Begin);
            while (_fileStream.Position != inode.CurrentInsertBlockBase)
            {
                var pointer = new byte[sizeof(uint)];
                _fileStream.Write(buffer, iterator, Disk.Structures.Super.BytesAvailablePerBlock);
                _fileStream.Read(pointer, 0, pointer.Length);
                _fileStream.Seek(BitConverter.ToUInt32(pointer, 0), SeekOrigin.Begin);
                iterator += Disk.Structures.Super.BytesAvailablePerBlock;
            }

            var difference = inode.NextRecordToInsertPointer - _fileStream.Position;
            _fileStream.Write(buffer, iterator, (int)difference);
        }

        public void UpdateRecordsWithSelection(Inode inode, List<UpdatedRecord> updatedRecords)
        {
            _fileStream.Seek(inode.DataBlockPointer, SeekOrigin.Begin);
            var bytesExplored = 0;
            var blockPointer = _fileStream.Position + Disk.Structures.Super.BytesAvailablePerBlock;

            foreach (var record in updatedRecords)
            {
                var bytesWritten = record.RecordNumber * inode.RecordSize;
                while (bytesExplored != bytesWritten)
                {
                    var distanceToPointer = blockPointer - _fileStream.Position;
                    if (bytesExplored + distanceToPointer < bytesWritten)
                    {
                        var pointerBuffer = new byte[sizeof(uint)];
                        bytesExplored += (int)distanceToPointer;
                        _fileStream.Seek(distanceToPointer, SeekOrigin.Current);
                        _fileStream.Read(pointerBuffer, 0, pointerBuffer.Length);
                        _fileStream.Seek(BitConverter.ToUInt32(pointerBuffer, 0), SeekOrigin.Begin);
                        blockPointer = _fileStream.Position + Disk.Structures.Super.BytesAvailablePerBlock;
                    }
                    else
                    {
                        var difference = (int)bytesWritten - bytesExplored;
                        bytesExplored += difference;
                        _fileStream.Seek(difference, SeekOrigin.Current);
                    }
                }

                var buffer = ConvertRecordValuesToByteArray(record.Record.Values, inode.Columns);
                var iterator = 0;
                var recordSizeLeft = inode.RecordSize - iterator;
                while (_fileStream.Position + recordSizeLeft > blockPointer)
                {
                    var pointerBuffer = new byte[sizeof(uint)];
                    var delta = blockPointer - _fileStream.Position;
                    _fileStream.Write(buffer, iterator, (int)delta);
                    _fileStream.Read(pointerBuffer, 0, pointerBuffer.Length);
                    _fileStream.Seek(BitConverter.ToUInt32(pointerBuffer, 0), SeekOrigin.Begin);
                    blockPointer = _fileStream.Position + Disk.Structures.Super.BytesAvailablePerBlock;
                    iterator += (int)delta;
                    bytesExplored += (int)delta;
                    recordSizeLeft = inode.RecordSize - iterator;
                }
                _fileStream.Write(buffer, iterator, (int)recordSizeLeft);
                bytesExplored += (int)recordSizeLeft;
            }
        }

        private SuperBlock ReadSuperBlock()
        {
            var buffer = new byte[SuperBlock.Size()];

            _fileStream.Read(buffer, 0, buffer.Length);
            return SuperBlock.GetFromBytes(buffer);
        }

        private byte[] ReadBitmap()
        {
            var buffer = new byte[Disk.Structures.Super.BitmapSize];

            _fileStream.Seek(Disk.Structures.Super.BitmapBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);
            _fileStream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        private DirectoryEntry[] ReadDirectory()
        {
            _fileStream.Seek(Disk.Structures.Super.DirectoryBlock * Disk.Structures.Super.BlockSize, 0);
            var iterator = 0;
            var entries = new DirectoryEntry[Disk.Structures.Super.TotalInodes];
            var buffer = new byte[Disk.Structures.Super.TotalInodes * DirectoryEntry.Size()];
            _fileStream.Read(buffer, 0, buffer.Length);

            for (var i = 0; i < Disk.Structures.Super.TotalInodes; i++)
            {
                entries[i] = new DirectoryEntry();
                entries[i].Name = ByteArrayToCharArray(buffer, DirectoryEntry.NameSize, iterator);
                iterator += DirectoryEntry.NameSize;
                entries[i].Available = BitConverter.ToBoolean(buffer, iterator);
                iterator++;
                entries[i].Inode = BitConverter.ToInt32(buffer, iterator);
                iterator += sizeof(int);
                entries[i].Number = BitConverter.ToInt32(buffer, iterator);
                iterator += sizeof(int);
            }

            return entries;
        }

        private Inode[] ReadInodeTable()
        {
            _fileStream.Seek(Disk.Structures.Super.InodeTableBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);
            var buffer = new byte[Disk.Structures.Super.TotalInodes * Inode.Size()];
            var inodes = new Inode[Disk.Structures.Super.TotalInodes];
            var iterator = 0;

            _fileStream.Read(buffer, 0, buffer.Length);
            for (var i = 0; i < inodes.Length; i++)
            {
                inodes[i] = Inode.GetFromByteArray(buffer, iterator);
                inodes[i].Columns = new List<ColumnMetadata>();
                iterator += Inode.Size();
            }

            foreach (var inode in inodes)
            {
                if (inode.TableInfoBlockPointer == 0) continue;

                var columnMetadataBuffer = new byte[ColumnMetadata.Size() * inode.ColumnCount];
                _fileStream.Seek(inode.TableInfoBlockPointer, SeekOrigin.Begin);
                _fileStream.Read(columnMetadataBuffer, 0, columnMetadataBuffer.Length);
                iterator = 0;

                for (var i = 0; i < inode.ColumnCount; i++)
                {
                    var columnName = ByteArrayToCharArray(columnMetadataBuffer, ColumnMetadata.NameSize, iterator);
                    iterator += ColumnMetadata.NameSize;
                    var typeName = ByteArrayToCharArray(columnMetadataBuffer, ColumnMetadata.TypeNameSize, iterator);
                    iterator += ColumnMetadata.TypeNameSize;
                    var typeSize = BitConverter.ToInt32(columnMetadataBuffer, iterator);
                    iterator += sizeof(int);

                    var meta = new ColumnMetadata { Name = columnName, Type = TypeFactory.GetType(new string(typeName).Replace("\0", string.Empty)) };
                    meta.Type.Size = typeSize;

                    inode.Columns.Add(meta);
                }
            }

            return inodes;
        }

        public void CloseStream()
        {
            _fileStream.Dispose();
        }

        private uint GetBlockPointerPosition(uint blockBase)
        {
            return blockBase + (uint)Disk.Structures.Super.BytesAvailablePerBlock;
        }

        private byte[] ConvertRecordValuesToByteArray(List<ValueNode> values, List<ColumnMetadata> columns)
        {
            var buffer = new List<byte>();

            for (var i = 0; i < columns.Count; i++)
            {
                if (!(values[i].Value is StringNode))
                {
                    buffer.AddRange(BitConverter.GetBytes(values[i].Value.Evaluate()));
                    continue;
                }

                var stringValue = values[i].Value.Evaluate();
                var byteArray = CharArrayToByteArray(stringValue, columns[i].Type.Size);
                buffer.AddRange(byteArray);
            }

            return buffer.ToArray();
        }

        private List<Record> ConvertByteArrayToRecords(byte[] buffer, List<ColumnMetadata> columns)
        {
            var records = new List<Record>();
            var iterator = 0;

            while (iterator < buffer.Length)
            {
                var record = new Record();
                foreach (var column in columns)
                {
                    if (column.Type.ToString() == "int")
                    {
                        var value = BitConverter.ToInt32(buffer, iterator);
                        record.Values.Add(new ValueNode { Value = new IntNode(value) });
                        iterator += sizeof(int);
                    }
                    else if (column.Type.ToString() == "char")
                    {
                        var value = ByteArrayToCharArray(buffer, column.Type.Size, iterator);
                        record.Values.Add(new ValueNode { Value = new StringNode(new string(value)) });
                        iterator += column.Type.Size;
                    }
                    else
                    {
                        var value = BitConverter.ToDouble(buffer, iterator);
                        record.Values.Add(new ValueNode { Value = new DoubleNode(value) });
                        iterator += sizeof(double);
                    }
                }

                records.Add(record);
            }

            return records;
        }

        private byte[] CharArrayToByteArray(char[] arr)
        {
            return arr.Select(t => (byte)t).ToArray();
        }

        private byte[] CharArrayToByteArray(string value, int size)
        {
            var buffer = new byte[size];
            for (var i = 0; i < value.Length; i++)
            {
                buffer[i] = (byte) value[i];
            }

            return buffer;
        }

        private char[] ByteArrayToCharArray(byte[] arr, int length, int position)
        {
            var buffer = new char[length];

            for (var i = 0; i < length; i++)
            {
                buffer[i] = (char)arr[position + i];
            }

            return buffer;
        }
    }
}
