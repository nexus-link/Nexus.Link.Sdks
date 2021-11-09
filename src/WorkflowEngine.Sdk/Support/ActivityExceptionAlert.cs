using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public class ActivityExceptionAlert
    {
        public IActivity Activity { get; set; }

        public ActivityExceptionCategoryEnum ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }
    }
}