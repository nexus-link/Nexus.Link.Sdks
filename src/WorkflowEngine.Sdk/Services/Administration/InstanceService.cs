using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration;

public class InstanceService : IInstanceService
{
    private readonly IRuntimeTables _runtimeTables;

    public InstanceService(IRuntimeTables runtimeTables)
    {
        _runtimeTables = runtimeTables;
    }

    public async Task<PageEnvelope<WorkflowInstance>> SearchAsync(WorkflowInstanceSearchDetails searchDetails, int offset = 0, int? limit = null, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(searchDetails, nameof(searchDetails));
        InternalContract.RequireValidated(searchDetails, nameof(searchDetails));

        var result = await _runtimeTables.WorkflowInstance.SearchAsync(searchDetails, offset, limit, cancellationToken);
        return new PageEnvelope<WorkflowInstance>
        {
            PageInfo = result.PageInfo,
            Data = result.Data.Select(x => new WorkflowInstance().From(x))
        };
    }
}