using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Support
{
    public class ActivityExceptionAlert
    {
        public IActivity Activity { get; set; }

        /// <summary>
        /// The category for this exception, e.g. Business
        /// </summary>
        public ActivityExceptionCategoryEnum ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }
    }
}