using System;

namespace ADO
{
    /// <summary>
    /// Exception class to manage recoverable problems.
    /// </summary>
    public class RecoverableException : Exception
    {
        public RecoverableException()
        {
        }

        public RecoverableException(string message) : base(message)
        {
        }

        public RecoverableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
