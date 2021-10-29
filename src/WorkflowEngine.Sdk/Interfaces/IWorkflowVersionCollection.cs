using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowVersionCollection
    {
        IWorkflowMgmtCapability Capability { get; }
        IAsyncRequestClient AsyncRequestClient { get; }
        WorkflowVersionCollection AddWorkflowVersion(IWorkflowVersion workflowVersion);
        WorkflowVersion<TResponse> SelectWorkflowVersion<TResponse>(int minVersion, int maxVersion);
        WorkflowVersion SelectWorkflowVersion(int minVersion, int maxVersion);
    }
}