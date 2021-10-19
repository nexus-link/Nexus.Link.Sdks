using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Model
{
    public class ActivityResult
    {
        public string Json { get; set; }

        public ActivityExceptionCategoryEnum? ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }
    }
}