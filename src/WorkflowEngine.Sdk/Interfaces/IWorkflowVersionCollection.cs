using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowVersionCollection
    {
        WorkflowVersion<TResponse> SelectWorkflowVersion<TResponse>(int minVersion, int maxVersion);
    }
}