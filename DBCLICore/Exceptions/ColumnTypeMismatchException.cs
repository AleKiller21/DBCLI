using System;

namespace DBCLICore.Exceptions
{
    [Serializable]
    public class ColumnTypeMismatchException : Exception
    {
        public ColumnTypeMismatchException(string columnType) : base($"One of the values inserted mismatched with a column of type double.")
        {
        }
    }
}