using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.LiteralNodes;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public class FileDatabaseStream
    {
        private BinaryWriter _writer;
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
            using (_writer = new BinaryWriter(File.Open(Disk.CurrentDatabase, FileMode.Open)))
            {
                if (inode.NextRecordToInsertPointer + inode.RecordSize < blockPointer)
                {
                    _writer.Seek((int)inode.NextRecordToInsertPointer, SeekOrigin.Begin);
                    _writer.Write(ConvertRecordValuesToByteArray(values, inode.Columns));
                    inode.NextRecordToInsertPointer = (uint)_writer.BaseStream.Position;
                    //WriteInode(inode);
                }
                else
                {
                    var remainingSpace = blockPointer - inode.NextRecordToInsertPointer;
                    var buffer = ConvertRecordValuesToByteArray(values, inode.Columns);
                    var newBlock = ManagerUtilities.GetBlocksFromBitmap(1);

                    //TODO Permitir escribir el registro recursivamente en multiples bloques en caso de que el RecordSize lo demande
                    _writer.Write(buffer, 0, (int)remainingSpace);
                    _writer.Write((uint)(newBlock[0] * Disk.Structures.Super.BlockSize));
                    _writer.Seek(newBlock[0] * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);
                    _writer.Write(buffer, (int)remainingSpace, buffer.Length - (int)remainingSpace);

                    inode.NextRecordToInsertPointer = (uint)_writer.BaseStream.Position;
                    inode.CurrentInsertBlockBase = (uint)(newBlock[0] * Disk.Structures.Super.BlockSize);
                    Disk.Structures.Super.UsedBlocks++;
                    Disk.Structures.Super.FreeBlocks--;
                }
            }

            WriteSuperBlock();
            WriteBitmap();
            WriteInode(inode);
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
                var byteArray = new byte[columns[i].Type.Size];
                for (var x = 0; x < stringValue.Length; x++)
                {
                    byteArray[x] = (byte) stringValue[x];
                }

                buffer.AddRange(byteArray);
            }

            return buffer.ToArray();
        }

        private byte[] CharArrayToByteArray(char[] arr)
        {
            return arr.Select(t => (byte)t).ToArray();
        }

        private char[] ByteArrayToCharArray(byte[] arr, int length, int position)
        {
            var buffer = new char[length];

            for (var i = 0; i < length; i++)
            {
                buffer[i] = (char) arr[position + i];
            }

            return buffer;
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
    }
}
