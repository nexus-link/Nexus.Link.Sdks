using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

/// <summary>
/// Management methods for a workflow
/// </summary>
public interface IWorkflowService : IRead<Workflow, string>
{
    /// <summary>
    /// Set a workflow in a cancelled state, which aborts the workflow process.
    /// </summary>
    Task CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry an workflow that is in a halted state
    /// </summary>
    Task RetryHaltedAsync(string workflowInstanceId, CancellationToken cancellationToken = default);
}