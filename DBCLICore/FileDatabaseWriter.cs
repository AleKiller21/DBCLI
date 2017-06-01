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

                var structures = new FileDatabaseStructures { Super = new SuperBlock(size, 512) };
                WriteStructuresToDisk(structures);
            }
        }

        private void WriteStructuresToDisk(FileDatabaseStructures structures)
        {
            WriteSuperBlock(structures.Super);
        }

        private void WriteSuperBlock(SuperBlock super)
        {
            _writer.Seek(0, SeekOrigin.Begin);

            _writer.Write(super.TotalBlocks);
            _writer.Write(super.FreeBlocks);
            _writer.Write(super.UsedBlocks);
            _writer.Write(super.BlockSize);
            _writer.Write(super.BytesAvailablePerBlock);
            _writer.Write(super.BitmapSize);
            _writer.Write(super.DirectorySize);
            _writer.Write(super.WordsInBitmap);
            _writer.Write(super.DatabaseSize);
            _writer.Write(super.TotalInodes);
            _writer.Write(super.FreeInodes);
            _writer.Write(super.BitmapBlock);
            _writer.Write(super.DirectoryBlock);
            _writer.Write(super.InodeTableBlock);
            _writer.Write(super.FirstDataBlock);
            _writer.Write(super.InodeTableSize);
        }
    }
}
