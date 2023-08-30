using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions
{
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