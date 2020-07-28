using System;
using Nexus.Link.AsyncCaller.Dispatcher.Models;

namespace Nexus.Link.AsyncCaller.Dispatcher.Exceptions
{
    internal class AssertionFailedException : GiveUpException
    {
        public AssertionFailedException(RequestEnvelope envelope, string message) 
            : base(envelope, message)
        {
        }

        public AssertionFailedException(RequestEnvelope envelope, string message, Exception innerException) 
            : base(envelope, message, innerException)
        {
        }
    }
}
