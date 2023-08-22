using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

/// <summary>
/// Management methods for instances
/// </summary>
public interface IInstanceService
{
    /// <summary>
    /// Fetch workflow instances based on search criterias and filters.
    /// </summary>
    Task<PageEnvelope<WorkflowInstance>> SearchAsync(WorkflowInstanceSearchDetails instanceSearchDetails, int offset = 0, int? limit = null, CancellationToken cancellationToken = default);
}