using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions
{
    /// <summary>
    /// Same as an <see cref="ActivityFailedException"/>, but we will ignore <see cref="ActivityFailUrgencyEnum"/> and fail the entire workflow.
    /// </summary>
    public class WorkflowFailedException : ActivityFailedException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exceptionCategory">The failure category</param>
        /// <param name="technicalMessage">A message directed to a developer</param>
        /// <param name="friendlyMessage">A message directed to a business person</param>
        public WorkflowFailedException(ActivityExceptionCategoryEnum exceptionCategory, string technicalMessage, string friendlyMessage) : base(exceptionCategory, technicalMessage, friendlyMessage)
        {
        }
    }
}