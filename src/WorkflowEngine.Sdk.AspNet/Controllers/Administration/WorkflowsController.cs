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
    [HttpGet("Workflows/{id}")]
    public async Task<Workflow> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        var workflow = await _capability.Workflow.ReadAsync(id, cancellationToken);
        return workflow;
    }

    /// <inheritdoc />
    [HttpPost("Workflows/{id}/Cancel")]
    public async Task CancelAsync(string id, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        await _capability.Workflow.CancelAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    [HttpPost("Workflows/{id}/RetryHalted")]
    public async Task RetryHaltedAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

        await _capability.Workflow.RetryHaltedAsync(workflowInstanceId, cancellationToken);
    }

}