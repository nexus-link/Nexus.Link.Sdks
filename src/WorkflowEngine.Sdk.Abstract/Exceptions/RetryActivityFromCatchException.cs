using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions
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