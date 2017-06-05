using System;

namespace DBCLICore.Exceptions
{
    [Serializable]
    public class RecordMismatchSelection : Exception
    {
        public RecordMismatchSelection() : base("No record with the given selection was found.")
        {
        }

        public RecordMismatchSelection(string message) : base(message)
        {
        }
    }
}