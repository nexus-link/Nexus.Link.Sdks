using System;
using Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Exceptions
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
