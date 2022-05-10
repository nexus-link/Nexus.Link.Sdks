using System;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Exceptions
{
    internal class DeadlineReachedException : Exception
    {
        public DeadlineReachedException(DateTimeOffset deadlineAt)
            : base($"Deadline was at {deadlineAt} and it is now {DateTimeOffset.Now}.")
        {
        }
    }
}
