using System;

namespace DBCLICore.Exceptions
{
    public class NotDivisibleByTwoException : Exception
    {
        public NotDivisibleByTwoException(long size) : base($"Database size {size} is not divisible by 2.")
        {
        }
    }
}