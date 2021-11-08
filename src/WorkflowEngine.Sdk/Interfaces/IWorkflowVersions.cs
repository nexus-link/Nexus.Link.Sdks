using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowVersions
    {
        IAsyncRequestMgmtCapability AsyncRequestMgmtCapability { get; }
        IWorkflowMgmtCapability WorkflowCapability { get; }
        string WorkflowCapabilityName { get; }
        string WorkflowFormId { get; }
        string WorkflowFormTitle { get; }
        WorkflowVersionCollection WorkflowVersionCollection { get; }
    }
}