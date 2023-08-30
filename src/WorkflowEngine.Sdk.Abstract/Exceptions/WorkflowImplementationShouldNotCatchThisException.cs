using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions
{
    /// <summary>
    /// This exception is transporting its inner exception up to the WorkflowExecutor.
    /// </summary>
    /// <remarks>A workflow implementation should always let this type of exception pass.</remarks>
    public class WorkflowImplementationShouldNotCatchThisException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">The exception that should be transported upwards.</param>
        public WorkflowImplementationShouldNotCatchThisException(Exception e) : base(null, e)
        {
        }
    }
}