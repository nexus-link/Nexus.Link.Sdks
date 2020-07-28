using System;

namespace Nexus.Link.AsyncCaller.Dispatcher.Exceptions
{
    internal class DeadlineReachedException : Exception
    {
        public DeadlineReachedException(DateTimeOffset deadLineAt)
            : base($"Deadline was at {deadLineAt} and it is now {DateTimeOffset.Now}.")
        {
        }
    }
}
