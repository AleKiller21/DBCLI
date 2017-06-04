using System;

namespace DBCLICore.Exceptions
{
    [Serializable]
    public class TableNotFoundException : Exception
    {
        public TableNotFoundException() : base("The specified table was not found.")
        {
        }

        public TableNotFoundException(string message) : base(message)
        {
        }
    }
}