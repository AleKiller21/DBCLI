using System;

namespace DBCLICore.Exceptions
{
    [Serializable]
    internal class MismatchInColumnAndValuesCount : Exception
    {
        public MismatchInColumnAndValuesCount() : base("The number of values does not match the number of columns of the specified table.")
        {
        }

        public MismatchInColumnAndValuesCount(string message) : base(message)
        {
        }
    }
}