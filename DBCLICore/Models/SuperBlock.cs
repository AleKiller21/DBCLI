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

        public static SuperBlock GetFromBytes(byte[] buffer)
        {
            var super = new SuperBlock();
            var iterator = 0;

            super.TotalBlocks = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.FreeBlocks = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.UsedBlocks = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.BlockSize = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.BytesAvailablePerBlock = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.BitmapSize = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.DirectorySize = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.DatabaseSize = BitConverter.ToInt64(buffer, iterator);
            iterator += sizeof(long);
            super.TotalInodes = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.FreeInodes = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.BitmapBlock = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.DirectoryBlock = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.InodeTableBlock = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.FirstDataBlock = BitConverter.ToInt32(buffer, iterator);
            iterator += sizeof(int);
            super.InodeTableSize = BitConverter.ToInt32(buffer, iterator);

            return super;
        }

        public byte[] ToByteArray()
        {
            var buffer = new List<byte>();

            buffer.AddRange(BitConverter.GetBytes(TotalBlocks));
            buffer.AddRange(BitConverter.GetBytes(FreeBlocks));
            buffer.AddRange(BitConverter.GetBytes(UsedBlocks));
            buffer.AddRange(BitConverter.GetBytes(BlockSize));
            buffer.AddRange(BitConverter.GetBytes(BytesAvailablePerBlock));
            buffer.AddRange(BitConverter.GetBytes(BitmapSize));
            buffer.AddRange(BitConverter.GetBytes(DirectorySize));
            buffer.AddRange(BitConverter.GetBytes(DatabaseSize));
            buffer.AddRange(BitConverter.GetBytes(TotalInodes));
            buffer.AddRange(BitConverter.GetBytes(FreeInodes));
            buffer.AddRange(BitConverter.GetBytes(BitmapBlock));
            buffer.AddRange(BitConverter.GetBytes(DirectoryBlock));
            buffer.AddRange(BitConverter.GetBytes(InodeTableBlock));
            buffer.AddRange(BitConverter.GetBytes(FirstDataBlock));
            buffer.AddRange(BitConverter.GetBytes(InodeTableSize));

            return buffer.ToArray();
        }

        public static int Size()
        {
            return 64;
        }
    }
}
