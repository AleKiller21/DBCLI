using System;
using System.Collections.Generic;
using System.Text;

namespace DBCLICore.Models
{
    public class Inode
    {
        public List<ColumnMetadata> Columns;
        public bool Available;
        public uint RecordSize;
        public uint RecordsAdded;
        public uint TableInfoBlockPointer;
        public uint DataBlockPointer;
        public uint NextRecordToInsertPointer;
        public uint CurrentInsertBlockBase;
        public int Number;
        public int ColumnCount;

        public static int Size()
        {
            return sizeof(int) * 8 + sizeof(bool);
        }
    }
}
