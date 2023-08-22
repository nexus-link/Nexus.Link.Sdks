using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

/// <summary>
/// Service for accessing persistence of <see cref="WorkflowSummary"/>
/// </summary>
public interface IWorkflowSummaryService
{
    /// <summary>
    /// Get the workflow summary for the workflow with instance id <paramref name="instanceId"/>
    /// </summary>
    Task<WorkflowSummary> GetSummaryAsync(string instanceId, CancellationToken cancellationToken = default);


    /// <summary>
    /// Get the workflow summary for the workflow with form id <paramref name="formId"/>, major version <paramref name="majorVersion"/> and instance id <paramref name="instanceId"/>
    /// </summary>
    Task<WorkflowSummary> GetSummaryAsync(string formId, int majorVersion, string instanceId, CancellationToken cancellationToken = default);
}