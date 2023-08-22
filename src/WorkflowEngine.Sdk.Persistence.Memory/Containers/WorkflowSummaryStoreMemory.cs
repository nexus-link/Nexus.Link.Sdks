using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Containers;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Containers;

/// <summary>
/// Service for storing the state for workflow instances, i.e. <see cref="WorkflowSummary"/>.
/// </summary>
public class WorkflowSummaryStoreMemory : CrudMemory<WorkflowSummary, string>, IWorkflowSummaryStore
{

    /// <summary>
    /// Use this in testing for throwing exceptions at create
    /// </summary>
    public Exception OnlyForTest_Create_AlwaysThrowThisException { get; set; }

    /// <summary>
    /// Use this in testing for throwing exceptions at update
    /// </summary>
    public Exception OnlyForTest_Update_AlwaysThrowThisException { get; set; }

    /// <inheritdoc />
    public async Task CreateOrUpdateAsync(string path, WorkflowSummary workflowSummary, CancellationToken cancellationToken = default)
    {
        try
        {
            if (OnlyForTest_Create_AlwaysThrowThisException != null) throw OnlyForTest_Create_AlwaysThrowThisException;
            await CreateWithSpecifiedIdAsync(path, workflowSummary, cancellationToken);
        }
        catch (FulcrumConflictException)
        {
            if (OnlyForTest_Update_AlwaysThrowThisException != null) throw OnlyForTest_Update_AlwaysThrowThisException;
            await UpdateAsync(path, workflowSummary, cancellationToken);
        }
    }
}