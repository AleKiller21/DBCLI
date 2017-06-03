using System;

namespace DBCLICore.Exceptions
{
    public class NotEnoughFreeInodesException : Exception
    {
        public NotEnoughFreeInodesException() : base("There are no free inodes left to create a new table.")
        {
        }

        public NotEnoughFreeInodesException(string message) : base(message)
        {
        }
    }
}