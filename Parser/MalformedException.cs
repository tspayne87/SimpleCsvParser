using System;

namespace SimpleCsvParser
{
    /// <summary>
    /// Exception is meant to deal with malformed strings that are found in the csv file or string.
    /// </summary>
    public class MalformedException : Exception
    {
        /// <summary>
        /// Constructor to be thrown with a message explaining what happened.
        /// </summary>
        /// <param name="message">The message explaining what happended with the malformity.</param>
        public MalformedException(string message)
            : base(message) { }
    }
}