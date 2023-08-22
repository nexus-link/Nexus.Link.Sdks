using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Containers;

/// <summary>
/// Service for storing the state for workflow instances, i.e. <see cref="WorkflowSummary"/>.
/// </summary>
public interface IWorkflowSummaryStore :
    ICreateWithSpecifiedId<WorkflowSummary, string>,
    IRead<WorkflowSummary, string>,
    IUpdate<WorkflowSummary, string>,
    IDelete<string>,
    Libraries.Crud.Interfaces.IDeleteAll
{
    /// <summary>
    /// Create or update a blob with path <paramref name="path"/> with content <paramref name="workflowSummary"/>
    /// </summary>
    /// <returns></returns>
    Task CreateOrUpdateAsync(string path, WorkflowSummary workflowSummary, CancellationToken cancellationToken = default);
}