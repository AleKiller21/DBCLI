using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBCLICore.Exceptions;
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
    }
}
