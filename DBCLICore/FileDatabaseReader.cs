using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBCLICore.Models;

namespace DBCLICore
{
    class FileDatabaseReader
    {
        private BinaryReader _reader;
        private string _path;

        public FileDatabaseStructures ConnectDatabase(string name)
        {
            _path = name;

            using (_reader = new BinaryReader(File.Open(_path, FileMode.Open)))
            {
                var structures = new FileDatabaseStructures { Super = ReadSuperBlock() };
                structures.BitMap = ReadBitmap(structures.Super);

                return structures;
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

        private byte[] ReadBitmap(SuperBlock super)
        {
            _reader.BaseStream.Position = super.BitmapBlock * super.BlockSize;
            return _reader.ReadBytes(super.BitmapSize);
        }
        
        //TODO ReadDirectory()
        //TODO ReadInodesTable()
    }
}
