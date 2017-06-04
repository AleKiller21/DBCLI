using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.TypeNodes;

namespace DBCLICore
{
    class FileDatabaseReader
    {
        private BinaryReader _reader;
        private string _path;

        public void ConnectDatabase(string name)
        {
            _path = name;
            using (_reader = new BinaryReader(File.Open(_path, FileMode.Open)))
            {
                Disk.Structures = new FileDatabaseStructures { Super = ReadSuperBlock() };
                Disk.Structures.BitMap = ReadBitmap();
                Disk.Structures.Directory = ReadDirectory();
                Disk.Structures.Inodes = ReadInodeTable();
            }
        }

        private SuperBlock ReadSuperBlock()
        {
            var totalBlocks = _reader.ReadInt32();
            var freeBlocks = _reader.ReadInt32();
            var usedBlocks = _reader.ReadInt32();
            var blockSize = _reader.ReadInt32();
            var bytesAvailablePerBlock = _reader.ReadInt32();
            var bitmapSize = _reader.ReadInt32();
            var directorySize = _reader.ReadInt32();
            var databaseSize = _reader.ReadInt64();
            var totalInodes = _reader.ReadInt32();
            var freeInodes = _reader.ReadInt32();
            var bitmapBlock = _reader.ReadInt32();
            var directoryBlock = _reader.ReadInt32();
            var inodeTableBlock = _reader.ReadInt32();
            var firstDataBlock = _reader.ReadInt32();
            var inodeTableSize = _reader.ReadInt32();

            return new SuperBlock
            {
                BlockSize = blockSize,
                TotalInodes = totalInodes,
                BitmapSize = bitmapSize,
                DirectorySize = directorySize,
                TotalBlocks = totalBlocks,
                InodeTableSize = inodeTableSize,
                InodeTableBlock = inodeTableBlock,
                FirstDataBlock = firstDataBlock,
                UsedBlocks = usedBlocks,
                DatabaseSize = databaseSize,
                DirectoryBlock = directoryBlock,
                BitmapBlock = bitmapBlock,
                FreeBlocks = freeBlocks,
                BytesAvailablePerBlock = bytesAvailablePerBlock,
                FreeInodes = freeInodes
            };
        }

        private byte[] ReadBitmap()
        {
            SetReaderPosition(Disk.Structures.Super.BitmapBlock * Disk.Structures.Super.BlockSize);
            return _reader.ReadBytes(Disk.Structures.Super.BitmapSize);
        }

        private DirectoryEntry[] ReadDirectory()
        {
            SetReaderPosition(Disk.Structures.Super.DirectoryBlock * Disk.Structures.Super.BlockSize);
            var entries = new DirectoryEntry[Disk.Structures.Super.TotalInodes];

            for (var i = 0; i < Disk.Structures.Super.TotalInodes; i++)
            {
                entries[i] =  new DirectoryEntry
                {
                    Name = _reader.ReadChars(DirectoryEntry.NameSize),
                    Available = _reader.ReadBoolean(),
                    Inode = _reader.ReadInt32(),
                    Number = _reader.ReadInt32()
                };
            }

            return entries;
        }

        private Inode[] ReadInodeTable()
        {
            SetReaderPosition(Disk.Structures.Super.InodeTableBlock * Disk.Structures.Super.BlockSize);

            var inodes = new Inode[Disk.Structures.Super.TotalInodes];
            for (var i = 0; i < inodes.Length; i++)
            {
                inodes[i] = new Inode
                {
                    Available = _reader.ReadBoolean(),
                    RecordSize = _reader.ReadUInt32(),
                    RecordsAdded = _reader.ReadUInt32(),
                    TableInfoBlockPointer = _reader.ReadUInt32(),
                    DataBlockPointer = _reader.ReadUInt32(),
                    NextRecordToInsertPointer = _reader.ReadUInt32(),
                    CurrentInsertBlockBase = _reader.ReadUInt32(),
                    Number = _reader.ReadInt32(),
                    ColumnCount = _reader.ReadInt32(),
                    Columns = new List<ColumnMetadata>()
                };
            }

            foreach (var inode in inodes)
            {
                if(inode.TableInfoBlockPointer == 0) continue;

                SetReaderPosition(inode.TableInfoBlockPointer);
                for (var i = 0; i < inode.ColumnCount; i++)
                {
                    var columnName = _reader.ReadChars(ColumnMetadata.NameSize);
                    var typeName = _reader.ReadChars(ColumnMetadata.TypeNameSize);
                    var typeSize = _reader.ReadInt32();

                    var meta = new ColumnMetadata { Name = columnName, Type = TypeFactory.GetType(new string(typeName).Replace("\0", string.Empty)) };
                    meta.Type.Size = typeSize;

                    inode.Columns.Add(meta);
                }
            }

            return inodes;
        }

        private void SetReaderPosition(long pos)
        {
            _reader.BaseStream.Position = pos;
        }
    }
}
