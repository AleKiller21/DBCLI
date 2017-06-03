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
            WriteSuperBlockFirstTime();
            WriteBitMapFirstTime();
            WriteDirectory();
            WriteInodeTable();
        }

        private void WriteSuperBlockFirstTime()
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

        private void WriteBitMapFirstTime()
        {
            _writer.Seek(Disk.Structures.Super.BitmapBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

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

            _writer.Write(Disk.Structures.BitMap);
        }

        private void WriteDirectory()
        {
            _writer.Seek(Disk.Structures.Super.DirectoryBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

            for (var i = 0; i < Disk.Structures.Super.TotalInodes; i++)
            {
                var entry = new DirectoryEntry{Number = i};

                _writer.Write(entry.Name);
                _writer.Write(entry.Available);
                _writer.Write(entry.Inode);
                _writer.Write(entry.Number);
            }
        }

        private void WriteInodeTable()
        {
            _writer.Seek(Disk.Structures.Super.InodeTableBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);

            for (var i = 0; i < Disk.Structures.Super.TotalInodes; i++)
            {
                var inode = new Inode{Available = true, Number = i};

                _writer.Write(inode.Available);
                _writer.Write(inode.RecordSize);
                _writer.Write(inode.RecordsAdded);
                _writer.Write(inode.TableInfoBlockPointer);
                _writer.Write(inode.DataBlockPointer);
                _writer.Write(inode.Number);
                _writer.Write(inode.ColumnCount);
            }
        }

        public void WriteSuperBlock()
        {
            using (_writer = new BinaryWriter(File.Open(Disk.CurrentDatabase, FileMode.Open)))
            {
                _writer.Seek(0, SeekOrigin.Begin);
                WriteSuperBlockFirstTime();
            }
        }

        public void WriteBitmap()
        {
            using (_writer = new BinaryWriter(File.Open(Disk.CurrentDatabase, FileMode.Open)))
            {
                _writer.Seek(Disk.Structures.Super.BitmapBlock * Disk.Structures.Super.BlockSize, SeekOrigin.Begin);
                _writer.Write(Disk.Structures.BitMap);
            }
        }

        public void WriteInode(Inode inode)
        {
            using (_writer = new BinaryWriter(File.Open(Disk.CurrentDatabase, FileMode.Open)))
            {
                var offset = Disk.Structures.Super.InodeTableBlock * Disk.Structures.Super.BlockSize +
                             inode.Number * Inode.Size();

                _writer.Seek(offset, SeekOrigin.Begin);
                _writer.Write(inode.Available);
                _writer.Write(inode.RecordSize);
                _writer.Write(inode.RecordsAdded);
                _writer.Write(inode.TableInfoBlockPointer);
                _writer.Write(inode.DataBlockPointer);
                _writer.Write(inode.Number);
                _writer.Write(inode.ColumnCount);

                //Solo habra un bloque para la metadata
                _writer.Seek((int)inode.TableInfoBlockPointer, SeekOrigin.Begin);
                foreach (var column in inode.Columns)
                {
                    var typeName = new char[ColumnMetadata.TypeNameSize];
                    column.Type.ToString().CopyTo(0, typeName, 0, column.Type.ToString().Length);

                    _writer.Write(column.Name);
                    _writer.Write(typeName);
                    _writer.Write(column.Type.Size);
                }
            }
        }

        public void WriteDirectoryEntry(DirectoryEntry entry)
        {
            using (_writer = new BinaryWriter(File.Open(Disk.CurrentDatabase, FileMode.Open)))
            {
                var offset = Disk.Structures.Super.DirectoryBlock * Disk.Structures.Super.BlockSize +
                             entry.Number * DirectoryEntry.Size();

                _writer.Seek(offset, SeekOrigin.Begin);
                _writer.Write(entry.Name);
                _writer.Write(entry.Available);
                _writer.Write(entry.Inode);
                _writer.Write(entry.Number);
            }
        }
    }
}
