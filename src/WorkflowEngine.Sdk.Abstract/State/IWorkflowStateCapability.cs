using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State;

/// <summary>
/// Services regarding the workflow state
/// </summary>
public interface IWorkflowStateCapability
{
    /// <summary>
    /// Service for dealing with with activity instances
    /// </summary>
    IActivityInstanceService ActivityInstance { get; }

    ILogService Log { get; }
    IWorkflowInstanceService WorkflowInstance { get; }

    /// <summary>
    /// Service for accessing database version of <see cref="Entities.WorkflowSummary"/>
    /// </summary>
    IWorkflowSummaryService WorkflowSummary { get; }

    /// <summary>
    /// Service for accessing storage version of <see cref="Entities.WorkflowSummary"/>
    /// </summary>
    IWorkflowSummaryServiceStorage WorkflowSummaryStorage { get; }
    IWorkflowSemaphoreService WorkflowSemaphore { get; }
}