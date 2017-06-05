using System;
using System.Runtime.Serialization;

namespace DBCLICore
{
    [Serializable]
    internal class ColumnNotFoundException : Exception
    {
        public ColumnNotFoundException() : base("The selected table doesn't have the specified column.")
        {
        }

        public ColumnNotFoundException(string message) : base(message)
        {
        }
    }
}