using WorkflowEngine.Sdk.WorkflowLogic;

namespace WorkflowEngine.Sdk.Interfaces
{
    public interface IWorkflowVersionCollection
    {
        WorkflowVersion<TResponse> SelectWorkflowVersion<TResponse>(int minVersion, int maxVersion);
    }
}