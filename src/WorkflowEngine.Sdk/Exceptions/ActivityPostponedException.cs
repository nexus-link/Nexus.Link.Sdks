using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    /// <summary>
    /// Throw this if you postpone your exception and will take care of waking it up.
    /// You can set <see cref="RequestPostponedException.TryAgainAfterMinimumTimeSpan"/> if you want your
    /// activity to be retried after a while, as a safe measure if you fail to wake the workflow up again.
    /// </summary>
    public class ActivityPostponedException : RequestPostponedException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ActivityPostponedException()
        {
        }
    }
}