using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBCLICore.Models;

namespace DBCLICore
{
    public class FileDatabaseWriter
    {
        private BinaryWriter _writer;

        public void CreateDatabase(string path, long size, int blockSize)
        {
            const int bufferSize = 1024;
            var buffer = new byte[bufferSize];

            using (_writer = new BinaryWriter(File.Open(path, FileMode.CreateNew)))
            {
                for (long i = 0; i < size; i += bufferSize)
                {
                    _writer.Write(buffer);
                }

                var structures = new FileDatabaseStructures { Super = new SuperBlock(size) };

                _writer.Seek(0, SeekOrigin.Begin);
                WriteStructuresToDisk(structures);
            }
        }

        private void WriteStructuresToDisk(FileDatabaseStructures structures)
        {
            WriteSuperBlock(structures.Super);
            WriteBitMap(structures);
            WriteDirectory(structures);
            WriteInodeTable(structures);
        }

        private void WriteSuperBlock(SuperBlock super)
        {
            _writer.Write(super.TotalBlocks);
            _writer.Write(super.FreeBlocks);
            _writer.Write(super.UsedBlocks);
            _writer.Write(super.BlockSize);
            _writer.Write(super.BytesAvailablePerBlock);
            _writer.Write(super.BitmapSize);
            _writer.Write(super.DirectorySize);
            _writer.Write(super.DatabaseSize);
            _writer.Write(super.TotalInodes);
            _writer.Write(super.FreeInodes);
            _writer.Write(super.BitmapBlock);
            _writer.Write(super.DirectoryBlock);
            _writer.Write(super.InodeTableBlock);
            _writer.Write(super.FirstDataBlock);
            _writer.Write(super.InodeTableSize);
        }

        private void WriteBitMap(FileDatabaseStructures structures)
        {
            _writer.Seek(structures.Super.BitmapBlock * structures.Super.BlockSize, SeekOrigin.Begin);

            structures.BitMap = new byte[structures.Super.BitmapSize];
            var blockCounter = 0;
            const byte msb = 128;

            for (var i = 0; i < structures.Super.BitmapSize; i++)
            {
                var word = Byte.MaxValue;

                for (byte bit = 0; bit < sizeof(byte) * 8; bit++)
                {
                    if (blockCounter == structures.Super.FirstDataBlock) break;
                    word ^= (byte) (msb >> bit);
                    blockCounter++;
                }

                structures.BitMap[i] = word;
            }

            _writer.Write(structures.BitMap);
        }

        private void WriteDirectory(FileDatabaseStructures structures)
        {
            _writer.Seek(structures.Super.DirectoryBlock * structures.Super.BlockSize, SeekOrigin.Begin);

            structures.Directory = new DirectoryEntry[structures.Super.TotalInodes];
            for (var i = 0; i < structures.Directory.Length; i++)
            {
                structures.Directory[i] = new DirectoryEntry();
            }

            foreach (var entry in structures.Directory)
            {
                _writer.Write(entry.Name);
                _writer.Write(entry.Available);
                _writer.Write(entry.Inode);
            }
        }

        private void WriteInodeTable(FileDatabaseStructures structures)
        {
            _writer.Seek(structures.Super.InodeTableBlock * structures.Super.BlockSize, SeekOrigin.Begin);

            var inodes = new Inode[structures.Super.TotalInodes];
            for (var i = 0; i < structures.Super.TotalInodes; i++)
            {
                inodes[i] = new Inode();

                _writer.Write(inodes[i].RecordSize);
                _writer.Write(inodes[i].RecordsAdded);
                _writer.Write(inodes[i].TableInfoBlockPointer);
                _writer.Write(inodes[i].DataBlockPointer);
            }
        }
    }
}
