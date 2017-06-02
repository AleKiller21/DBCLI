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

                Disk.Structures = new FileDatabaseStructures { Super = new SuperBlock(size) };

                _writer.Seek(0, SeekOrigin.Begin);
                WriteStructuresToDisk();
                Disk.Structures = null;
            }
        }

        private void WriteStructuresToDisk()
        {
            WriteSuperBlock();
            WriteBitMap();
            WriteDirectory();
            WriteInodeTable();
        }

        private void WriteSuperBlock()
        {
            _writer.Write(Disk.Structures.Super.TotalBlocks);
            _writer.Write(Disk.Structures.Super.FreeBlocks);
            _writer.Write(Disk.Structures.Super.UsedBlocks);
            _writer.Write(Disk.Structures.Super.BlockSize);
            _writer.Write(Disk.Structures.Super.BytesAvailablePerBlock);
            _writer.Write(Disk.Structures.Super.BitmapSize);
            _writer.Write(Disk.Structures.Super.DirectorySize);
            _writer.Write(Disk.Structures.Super.DatabaseSize);
            _writer.Write(Disk.Structures.Super.TotalInodes);
            _writer.Write(Disk.Structures.Super.FreeInodes);
            _writer.Write(Disk.Structures.Super.BitmapBlock);
            _writer.Write(Disk.Structures.Super.DirectoryBlock);
            _writer.Write(Disk.Structures.Super.InodeTableBlock);
            _writer.Write(Disk.Structures.Super.FirstDataBlock);
            _writer.Write(Disk.Structures.Super.InodeTableSize);
        }

        private void WriteBitMap()
        {
            _writer.Seek(Disk.Structures.Super.BitmapBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

            Disk.Structures.BitMap = new byte[Disk.Structures.Super.BitmapSize];
            var blockCounter = 0;
            const byte msb = 128;

            for (var i = 0; i < Disk.Structures.Super.BitmapSize; i++)
            {
                var word = Byte.MaxValue;

                for (byte bit = 0; bit < sizeof(byte) * 8; bit++)
                {
                    if (blockCounter == Disk.Structures.Super.FirstDataBlock) break;
                    word ^= (byte) (msb >> bit);
                    blockCounter++;
                }

                Disk.Structures.BitMap[i] = word;
            }

            _writer.Write(Disk.Structures.BitMap);
        }

        private void WriteDirectory()
        {
            _writer.Seek(Disk.Structures.Super.DirectoryBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

            Disk.Structures.Directory = new DirectoryEntry[Disk.Structures.Super.TotalInodes];
            for (var i = 0; i < Disk.Structures.Directory.Length; i++)
            {
                Disk.Structures.Directory[i] = new DirectoryEntry();
            }

            foreach (var entry in Disk.Structures.Directory)
            {
                _writer.Write(entry.Name);
                _writer.Write(entry.Available);
                _writer.Write(entry.Inode);
            }
        }

        private void WriteInodeTable()
        {
            _writer.Seek(Disk.Structures.Super.InodeTableBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

            var inodes = new Inode[Disk.Structures.Super.TotalInodes];
            for (var i = 0; i < Disk.Structures.Super.TotalInodes; i++)
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
