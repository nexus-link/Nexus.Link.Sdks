using System;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions
{
    /// <summary>
    /// Throw this if you postpone your workflow for a while..
    /// Set <see cref="RequestPostponedException.TryAgainAfterMinimumTimeSpan"/> if you want help
    /// to be woken up after a while.
    /// </summary>
    public class WorkflowPostponedException : RequestPostponedException
    {
        /// <summary>
        /// Throw this if you postpone your workflow for a while..
        /// Set <paramref name="tryAgainAfterMinimumTimeSpan"/> if you want to be woken up after a specific time.
        /// </summary>
        public WorkflowPostponedException(TimeSpan? tryAgainAfterMinimumTimeSpan) : base(tryAgainAfterMinimumTimeSpan)
        {
        }
    }
}