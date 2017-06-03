using System;

namespace DBCLICore.Exceptions
{
    public class NoDirectoryEntryAvailableException : Exception
    {
        public NoDirectoryEntryAvailableException() : base("There are no entries available to create the table.")
        {
        }

        public NoDirectoryEntryAvailableException(string message) : base(message)
        {
        }
    }
}