using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

internal class WorkflowContainerSingleVersion : WorkflowContainer
{
    /// <inheritdoc />
    public WorkflowContainerSingleVersion(string capabilityName, string workflowTitle, string workflowId, IWorkflowEngineRequiredCapabilities workflowCapabilities) 
        : base(capabilityName, workflowTitle, workflowId, workflowCapabilities)
    {
    }
}