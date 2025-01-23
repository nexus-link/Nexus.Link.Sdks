using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration;

/// <inheritdoc cref="IWorkflowService" />
public abstract class WorkflowsController : ControllerBase, IWorkflowService
{
    private readonly IWorkflowMgmtCapability _capability;

    /// <summary>
    /// Controller
    /// </summary>
    protected WorkflowsController(IWorkflowMgmtCapability capability)
    {
        _capability = capability;
    }

    /// <inheritdoc />
    [HttpGet("Workflows/{workflowInstanceId}")]
    public async Task<Workflow> ReadAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

        var workflow = await _capability.Workflow.ReadAsync(workflowInstanceId, cancellationToken);
        return workflow;
    }

    /// <inheritdoc />
    [HttpPost("Workflows/{workflowInstanceId}/Cancel")]
    public async Task CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

        await _capability.Workflow.CancelAsync(workflowInstanceId, cancellationToken);
    }

    /// <inheritdoc />
    [HttpPost("Workflows/{workflowInstanceId}/RetryHalted")]
    public async Task RetryHaltedAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

        await _capability.Workflow.RetryHaltedAsync(workflowInstanceId, cancellationToken);
    }

}