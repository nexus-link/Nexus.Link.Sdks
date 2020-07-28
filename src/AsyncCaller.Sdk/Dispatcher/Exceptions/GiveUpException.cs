using System;
using Nexus.Link.AsyncCaller.Dispatcher.Models;

namespace Nexus.Link.AsyncCaller.Dispatcher.Exceptions
{
    internal class GiveUpException : Exception
    {
        public GiveUpException(RequestEnvelope envelope, string message) : base(message)
        {
        }

        public GiveUpException(RequestEnvelope envelope, string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
