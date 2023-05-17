using System;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    /// <summary>
    /// Throw this from inside a catch statement when you want the failed activity to be tried again
    /// </summary>
    public class RetryActivityFromCatchException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RetryActivityFromCatchException()
        {
        }
    }
}