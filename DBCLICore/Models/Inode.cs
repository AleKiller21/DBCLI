using System;
using System.Collections.Generic;
using System.Text;

namespace DBCLICore.Models
{
    public class Inode
    {
        public List<ColumnMetadata> Columns;
        public uint RecordSize;
        public uint RecordsAdded;
        public uint TableInfoBlockPointer;
        public uint DataBlockPointer;

        public static int Size()
        {
            return sizeof(int) * 4;
        }
    }
}
