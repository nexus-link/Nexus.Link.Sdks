using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Model
{
    public class ActivityResult
    {
        public string Json { get; set; }

        public ActivityExceptionCategoryEnum? ExceptionCategory { get; set; }

        public ActivityFailUrgencyEnum? FailUrgency { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }
        public ActivityStateEnum State { get; set; }
    }
}