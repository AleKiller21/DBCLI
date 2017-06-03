using System.Collections.Generic;
using System.Linq;
using DBCLICore.Exceptions;
using DBCLICore.Models;
using SqlParser.SyntaxAnalyser.Nodes.StatementNodes.CreateNodes;

namespace DBCLICore
{
    public static class ManagerUtilities
    {
        private const string Path = "..\\Databases\\";

        public static List<int> GetBlocksFromBitmap(int amount)
        {
            var wordsOccupied = 0;
            var extractedBlocks = new List<int>();
            const int msb = 128;

            for (var i = 0; i < Disk.Structures.BitMap.Length; i++)
            {
                if (Disk.Structures.BitMap[i] == 0)
                {
                    wordsOccupied++;
                    continue;
                }

                for (var bit = 0; bit < sizeof(byte) * 8; bit++)
                {
                    var block = Disk.Structures.BitMap[i] & (msb >> bit);
                    if (block == 0) continue;

                    extractedBlocks.Add(sizeof(byte) * 8 * wordsOccupied + bit);
                    Disk.Structures.BitMap[i] ^= (byte)(msb >> bit);
                    if (extractedBlocks.Count == amount) break;
                }

                if (extractedBlocks.Count == amount) break;
            }

            if (extractedBlocks.Count == 0) throw new NotEnoughFreeBlocksException();

            Disk.Structures.Super.FreeBlocks -= amount;
            Disk.Structures.Super.UsedBlocks += amount;

            return extractedBlocks;
        }

        public static Inode SetUpInode(List<int> blocks, List<ColumnNode> columns)
        {
            foreach (var inode in Disk.Structures.Inodes)
            {
                if(!inode.Available) continue;

                inode.Available = false;
                inode.DataBlockPointer = (uint) (blocks[0] * Disk.Structures.Super.BlockSize);
                inode.TableInfoBlockPointer = (uint) (blocks[1] * Disk.Structures.Super.BlockSize);
                inode.RecordSize = (uint) ManagerUtilities.CalculateRecordSize(columns);
                inode.Columns = SetUpInodeColumns(columns);
                inode.ColumnCount = columns.Count;

                Disk.Structures.Super.FreeInodes--;
                return inode;
            }

            throw new NotEnoughFreeInodesException();
        }

        public static DirectoryEntry SetUpDirectoryEntry(string tableName, Inode inode)
        {
            foreach (var entry in Disk.Structures.Directory)
            {
                if(!entry.Available) continue;

                tableName.CopyTo(0, entry.Name, 0, tableName.Length);
                entry.Available = false;
                entry.Inode = inode.Number;
                
                return entry;
            }

            throw new NoDirectoryEntryAvailableException();
        }

        public static DirectoryEntry GetDirectoryEntry(string tableName)
        {
            return Disk.Structures.Directory.First(entry => CharArrayToString(entry.Name).Equals(tableName));
        }

        public static Inode GetInode(int inumber)
        {
            return Disk.Structures.Inodes.First(inode => inode.Number == inumber);
        }

        private static string CharArrayToString(char[] array)
        {
            return new string(array).Replace("\0", string.Empty);
        }

        public static bool CheckFreeSpace()
        {
            return Disk.Structures.Super.FreeBlocks >= 2 && Disk.Structures.Super.FreeInodes >= 1;
        }

        public static long ConvertToBytes(long size, UnitSize unit)
        {
            size *= 1024;
            if (unit == UnitSize.Mb) return size * 1024;
            return size * 1024 * 1024;
        }

        public static bool CheckIfSizeDivisibleByTwo(long size)
        {
            return size % 2 == 0;
        }

        public static string GetQualifiedName(string name)
        {
            return Path + name + ".db";
        }

        private static List<ColumnMetadata> SetUpInodeColumns(List<ColumnNode> columns)
        {
            var metadatas = new List<ColumnMetadata>();

            foreach (var column in columns)
            {
                var metadata = new ColumnMetadata();
                column.Name.ToString().CopyTo(0, metadata.Name, 0, column.Name.ToString().Length);
                metadata.Type = column.Type;

                metadatas.Add(metadata);
            }

            return metadatas;
        }

        private static int CalculateRecordSize(List<ColumnNode> columns)
        {
            return columns.Sum(column => column.Type.Size);
        }

        public static void FreeBlocks(List<int> blocks)
        {
            foreach (var block in blocks)
            {
                var bytePosition = block / 8;
                var blockCounter = bytePosition * 8;
                var msb = 128;

                for (var i = 0; i < 8; i++)
                {
                    if (blockCounter != block)
                    {
                        blockCounter++;
                        continue;
                    }

                    Disk.Structures.BitMap[bytePosition] ^= (byte)(msb >> i);
                    Disk.Structures.Super.FreeBlocks++;
                    Disk.Structures.Super.UsedBlocks--;
                    break;
                }
            }
        }
    }
}
