using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

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
    [HttpGet("Instances/{id}")]
    public async Task<Workflow> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        var workflow = await _capability.Workflow.ReadAsync(id, cancellationToken);
        return workflow;
    }

    /// <inheritdoc />
    [HttpPost("Instances/{id}/Cancel")]
    public async Task CancelAsync(string id, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

        await _capability.Workflow.CancelAsync(id, cancellationToken);
    }

    [HttpPost("Instances")]
    public async Task<PageEnvelope<Workflow>> SearchAsync(WorkflowSearchDetails searchDetails, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNull(searchDetails, nameof(searchDetails));
        ServiceContract.RequireValidated(searchDetails, nameof(searchDetails));

        return await _capability.Workflow.SearchAsync(searchDetails, offset, limit, cancellationToken);
    }
       
}