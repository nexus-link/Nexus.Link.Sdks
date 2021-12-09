using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowVersions
    {
        IWorkflowEngineRequiredCapabilities WorkflowCapabilities { get; }
        string WorkflowCapabilityName { get; }
        string WorkflowFormId { get; }
        string WorkflowFormTitle { get; }
        WorkflowVersionCollection WorkflowVersionCollection { get; }
        ActivityDefinition GetActivityDefinition(string activityFormId);
    }
}