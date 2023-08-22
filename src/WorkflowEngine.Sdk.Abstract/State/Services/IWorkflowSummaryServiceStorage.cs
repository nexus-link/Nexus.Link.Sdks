using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

/// <summary>
/// Service for accessing persistence of <see cref="WorkflowSummary"/>
/// </summary>
public interface IWorkflowSummaryServiceStorage
{
    /// <summary>
    /// Read the blob for the workflow instance with id <paramref name="workflowInstanceId"/>.
    /// </summary>
    Task<WorkflowSummary> ReadBlobAsync(string workflowInstanceId, DateTimeOffset instanceStartedAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a blob for <paramref name="workflowSummary"/>
    /// </summary>
    Task WriteBlobAsync(WorkflowSummary workflowSummary, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete the blob for the workflow instance with id <paramref name="workflowInstanceId"/>.
    /// </summary>
    Task DeleteBlobAsync(string workflowInstanceId, DateTimeOffset instanceStartedAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the path to a workflow summary, based on the <paramref name="workflowInstanceId"/>
    /// </summary>
    public static string GetWorkflowSummaryPath(string workflowInstanceId)
    {
        return $"{workflowInstanceId}.json";
    }
}