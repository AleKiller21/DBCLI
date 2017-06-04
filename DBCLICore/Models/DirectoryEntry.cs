using System;
using System.Collections.Generic;
using System.Text;

namespace DBCLICore.Models
{
    public class DirectoryEntry
    {
        public char[] Name;
        public int Inode;
        public int Number;
        public bool Available;
        public const int NameSize = 100;

        public DirectoryEntry()
        {
            Name = new char[NameSize];
            Inode = -1;
            Available = true;
        }

        public byte[] ToByteArray()
        {
            var buffer = new List<byte>();
            var byteCharArray = new byte[NameSize];

            for (var i = 0; i < Name.Length; i++)
            {
                byteCharArray[i] = (byte)Name[i];
            }
            buffer.AddRange(byteCharArray);
            buffer.AddRange(BitConverter.GetBytes(Available));
            buffer.AddRange(BitConverter.GetBytes(Inode));
            buffer.AddRange(BitConverter.GetBytes(Number));

            return buffer.ToArray();
        }

        public static int Size()
        {
            return sizeof(int)*2 + sizeof(bool) + NameSize;
        }
    }
}
