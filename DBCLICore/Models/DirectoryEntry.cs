using System;
using System.Collections.Generic;
using System.Text;

namespace DBCLICore.Models
{
    public class DirectoryEntry
    {
        public char[] Name;
        public int Inode;
        public bool Available;
        public const int NameSize = 100;

        public DirectoryEntry()
        {
            Name = new char[NameSize];
            Inode = -1;
            Available = true;
        }

        public static int Size()
        {
            return sizeof(int) + sizeof(bool) + NameSize * sizeof(char);
        }
    }
}
