using System;
using System.Collections.Generic;
using System.Text;

namespace DBCLICore.Models
{
    public class SuperBlock
    {
        public int TotalBlocks;
        public int FreeBlocks;
        public int UsedBlocks;
        public int BlockSize;
        public int BytesAvailablePerBlock;
        public int BitmapSize;
        public int DirectorySize;
        public long DatabaseSize;
        public int TotalInodes;
        public int FreeInodes;
        public int BitmapBlock;
        public int DirectoryBlock;
        public int InodeTableBlock;
        public int FirstDataBlock;
        public int InodeTableSize;

        public SuperBlock()
        {

        }

        public SuperBlock(long databaseSize)
        {
            DatabaseSize = databaseSize;
            BlockSize = 4096;
            BytesAvailablePerBlock = BlockSize - sizeof(int);
            TotalBlocks = (int) (DatabaseSize / BlockSize);
            TotalInodes = BlockSize / Inode.Size();
            FreeInodes = TotalInodes;
            BitmapBlock = 1;
            BitmapSize = TotalBlocks / 8;
            DirectorySize = TotalInodes * DirectoryEntry.Size();
            InodeTableSize = TotalInodes * Inode.Size();
            DirectoryBlock = (int)Math.Ceiling(BitmapSize / (double)BlockSize + BitmapBlock);
            InodeTableBlock = (int)Math.Ceiling(DirectorySize / (double)BlockSize) + DirectoryBlock;
            FirstDataBlock = InodeTableBlock + (int) Math.Ceiling((double) InodeTableSize / BlockSize);
            UsedBlocks = FirstDataBlock;
            FreeBlocks = TotalBlocks - UsedBlocks;
        }
    }
}
