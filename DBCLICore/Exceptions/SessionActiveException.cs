using System;

namespace DBCLICore.Exceptions
{
    public class SessionActiveException : Exception
    {
        public SessionActiveException() : base("You must end your session before proceeding.")
        {
        }

        public SessionActiveException(string message) : base(message)
        {
        }
    }
}