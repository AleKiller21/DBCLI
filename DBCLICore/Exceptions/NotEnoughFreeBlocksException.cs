using System;

namespace DBCLICore.Exceptions
{
    public class NotEnoughFreeBlocksException : Exception
    {
        public NotEnoughFreeBlocksException() : base("Not enough free blocks to proceed with operation.")
        {
        }

        public NotEnoughFreeBlocksException(string message) : base(message)
        {
        }
    }
}