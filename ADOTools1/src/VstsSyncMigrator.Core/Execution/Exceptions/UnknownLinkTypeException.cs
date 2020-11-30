using System;

namespace VstsSyncMigrator.Engine.Execution.Exceptions
{
    public class UnknownLinkTypeException : Exception
    {
        public UnknownLinkTypeException(string message) : base(message)
        {

        }
    }
}
