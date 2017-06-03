﻿using System;

namespace DBCLICore.Exceptions
{
    public class SessionNotCreatedException : Exception
    {
        public SessionNotCreatedException() : base("You are currently not participating in any active session.")
        {
        }

        public SessionNotCreatedException(string message) : base(message)
        {
        }
    }
}