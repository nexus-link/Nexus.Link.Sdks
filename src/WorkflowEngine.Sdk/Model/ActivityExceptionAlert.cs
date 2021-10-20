using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Model
{
    public class ActivityExceptionAlert
    {
        public string WorkflowInstanceId { get; set; }
        public string ActivityInstanceId { get; set; }

        public ActivityExceptionCategoryEnum ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }
    }
}