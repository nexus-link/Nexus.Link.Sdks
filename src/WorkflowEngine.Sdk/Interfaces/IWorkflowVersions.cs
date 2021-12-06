using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowVersions
    {
        IAsyncRequestMgmtCapability AsyncRequestMgmtCapability { get; }
        IWorkflowConfigurationCapability ConfigurationCapability{ get; }
        IWorkflowStateCapability StateCapability { get; }
        string WorkflowCapabilityName { get; }
        string WorkflowFormId { get; }
        string WorkflowFormTitle { get; }
        WorkflowVersionCollection WorkflowVersionCollection { get; }
    }
}