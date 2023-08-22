using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Azure.Storage.V12.Blob;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Containers;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Containers;

/// <inheritdoc cref="IWorkflowSummaryStore" />
public class WorkflowSummaryStore : CrudAzureStorageContainer<WorkflowSummary>, IWorkflowSummaryStore
{

    /// <inheritdoc />
    public WorkflowSummaryStore(string connectionString, string containerName) : base(connectionString, containerName)
    {
    }

    /// <inheritdoc />
    public async Task CreateOrUpdateAsync(string path, WorkflowSummary workflowSummary, CancellationToken cancellationToken = default)
    {
        try
        {
            await CreateWithSpecifiedIdAsync(path, workflowSummary, cancellationToken);
        }
        catch (FulcrumConflictException)
        {
            await UpdateAsync(path, workflowSummary, cancellationToken);
        }
    }
}