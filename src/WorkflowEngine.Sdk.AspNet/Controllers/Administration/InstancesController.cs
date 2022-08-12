using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration;

/// <inheritdoc cref="IWorkflowService" />
public abstract class InstancesController : ControllerBase, IInstanceService
{
    private readonly IWorkflowMgmtCapability _capability;

    /// <summary>
    /// Controller
    /// </summary>
    protected InstancesController(IWorkflowMgmtCapability capability)
    {
        _capability = capability;
    }

    /// <inheritdoc />
    [HttpPost("Instances")]
    public async Task<PageEnvelope<WorkflowInstance>> SearchAsync(WorkflowInstanceSearchDetails searchDetails, int offset = 0, int? limit = null, CancellationToken cancellationToken = default)
    {
        ServiceContract.RequireNotNull(searchDetails, nameof(searchDetails));
        ServiceContract.RequireValidated(searchDetails, nameof(searchDetails));

        return await _capability.Instance.SearchAsync(searchDetails, offset, limit, cancellationToken);
    }
}