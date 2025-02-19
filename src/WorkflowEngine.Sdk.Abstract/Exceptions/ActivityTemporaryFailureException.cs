using System;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions
{
    /// <summary>
    /// Throw this if you have a temporary failure and you want the retries to use a backoff strategy.
    /// </summary>
    public class ActivityTemporaryFailureException : ActivityPostponedException
    {
        /// <summary>
        /// Throw this if you have a temporary failure and you want the retries to use a backoff strategy.
        /// Set <paramref name="tryAgainAfterMinimumTimeSpan"/> if you want a recommended minimum time for the first backoff.
        /// </summary>
        public ActivityTemporaryFailureException(TimeSpan? tryAgainAfterMinimumTimeSpan) : base(tryAgainAfterMinimumTimeSpan)
        {
            Backoff = true;
        }
    }
}