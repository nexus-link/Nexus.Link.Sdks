using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
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