using System;
using System.Runtime.Serialization;

namespace DBCLICore
{
    [Serializable]
    public class ColumnSizeMismatchException : Exception
    {
        public ColumnSizeMismatchException()
        {
        }

        public ColumnSizeMismatchException(int columnSize) : base($"The size of value in record was bigger than the size of the column which is of {columnSize}.")
        {
        }
    }
}