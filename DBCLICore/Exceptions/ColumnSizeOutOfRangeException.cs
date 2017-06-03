using System;

namespace DBCLICore.Exceptions
{
    [Serializable]
    internal class ColumnSizeOutOfRangeException : Exception
    {
        public ColumnSizeOutOfRangeException() : base("The size specified for the column of type char exceeds the maximum limit of 4000.")
        {
        }

        public ColumnSizeOutOfRangeException(string message) : base(message)
        {
        }
    }
}