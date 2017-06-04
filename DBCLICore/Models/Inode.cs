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

        public byte[] ToByteArray()
        {
            var buffer = new List<byte>();

            buffer.AddRange(BitConverter.GetBytes(Available));
            buffer.AddRange(BitConverter.GetBytes(RecordSize));
            buffer.AddRange(BitConverter.GetBytes(RecordsAdded));
            buffer.AddRange(BitConverter.GetBytes(TableInfoBlockPointer));
            buffer.AddRange(BitConverter.GetBytes(DataBlockPointer));
            buffer.AddRange(BitConverter.GetBytes(NextRecordToInsertPointer));
            buffer.AddRange(BitConverter.GetBytes(CurrentInsertBlockBase));
            buffer.AddRange(BitConverter.GetBytes(Number));
            buffer.AddRange(BitConverter.GetBytes(ColumnCount));

            return buffer.ToArray();
        }

        public static Inode GetFromByteArray(byte[] buffer, int offset)
        {
            var inode = new Inode();

            inode.Available = BitConverter.ToBoolean(buffer, offset++);
            inode.RecordSize = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);
            inode.RecordsAdded = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);
            inode.TableInfoBlockPointer = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);
            inode.DataBlockPointer = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);
            inode.NextRecordToInsertPointer = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);
            inode.CurrentInsertBlockBase = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);
            inode.Number = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);
            inode.ColumnCount = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);

            return inode;
        }
    }
}
