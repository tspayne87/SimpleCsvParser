using System;

namespace Parser
{
    public class MalformedException : Exception
    {
        public MalformedException(string message)
            : base(message) { }
    }
}