using System;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions
{
    /// <summary>
    /// Throw this if the activity needs the response of a request to finish before it can continue.
    /// </summary>
    public class ActivityWaitsForRequestException: RequestPostponedException
    {
        /// <summary>
        /// Throw this if the activity needs the response of a request to finish before it can continue.
        /// </summary>
        public ActivityWaitsForRequestException(string waitingForRequestId) : base(waitingForRequestId)
        {
        }
    }
}