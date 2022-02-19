using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State;

/// <inheritdoc />
public class WorkflowSemaphoreService : IWorkflowSemaphoreService
{
    private readonly IRuntimeTables _runtimeTables;

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkflowSemaphoreService(IRuntimeTables runtimeTables)
    {
        _runtimeTables = runtimeTables;
    }

    /// <inheritdoc />
    public async Task<string> CreateOrTakeOverOrEnqueueAsync(WorkflowSemaphoreCreate item, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNull(item, nameof(item));
        InternalContract.RequireValidated(item, nameof(item));
        var recordCreate = new WorkflowSemaphoreRecordCreate().From(item);
        var count = 0;
        while (count < 3)
        {
            count++;
            try
            {
                var idAsGuid = await _runtimeTables.WorkflowSemaphore.CreateAsync(recordCreate, cancellationToken);
                return idAsGuid.ToGuidString();
            }
            catch (FulcrumConflictException)
            {
                // The semaphore record already exists
                var unique = new WorkflowSemaphoreRecordUnique().From(item);
                var searchDetails = new SearchDetails<WorkflowSemaphoreRecord>(unique);
                var record =
                    await _runtimeTables.WorkflowSemaphore.FindUniqueAsync(searchDetails, cancellationToken);
                if (record == null)
                {
                    // Try again, this should be a good time to create a new record.
                    continue;
                }
                if (record.WorkflowInstanceId == recordCreate.WorkflowInstanceId
                    || !record.Raised
                    || record.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    record.WorkflowFormId = recordCreate.WorkflowInstanceId;
                    record.Raised = true;
                    record.ExpiresAt = recordCreate.ExpiresAt;
                    try
                    {
                        await _runtimeTables.WorkflowSemaphore.UpdateAsync(record.Id, record, cancellationToken);
                        await DeleteThisInstanceFromQueue(record, cancellationToken);
                    }
                    catch (FulcrumConflictException)
                    {
                        // Someone else just raised the semaphore, try again
                        continue;
                    }
                    return record.Id.ToGuidString();
                }
                // The semaphore was already raised by another workflow instance.
                // Put this workflow instance on the waiting list.
                var queueItemCreate = new WorkflowSemaphoreQueueRecordCreate
                {
                    WorkflowSemaphoreId = record.Id,
                    WorkflowInstanceId = recordCreate.WorkflowInstanceId
                };
                try
                {
                    await _runtimeTables.WorkflowSemaphoreQueue.CreateAsync(queueItemCreate, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // The queue item already exists, this is OK.
                }
                throw new RequestPostponedException();
            }
        }
        throw new FulcrumTryAgainException(
            $"Could not establish the semaphore {item} in three retries.");
    }

    private async Task DeleteThisInstanceFromQueue(WorkflowSemaphoreRecord record, CancellationToken cancellationToken)
    {
        var searchDetails2 = new SearchDetails<WorkflowSemaphoreQueueRecord>(new WorkflowSemaphoreQueueRecordUnique
        {
            WorkflowSemaphoreId = record.Id,
            WorkflowInstanceId = record.WorkflowInstanceId
        });
        var queueItem = await _runtimeTables.WorkflowSemaphoreQueue.FindUniqueAsync(searchDetails2, cancellationToken);
        if (queueItem != null)
        {
            await _runtimeTables.WorkflowSemaphoreQueue.DeleteAsync(queueItem.Id, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task UpdateExpirationAsync(string id, string workflowInstanceId, DateTimeOffset expireAt,
        CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
        InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
        InternalContract.RequireGreaterThan(DateTimeOffset.UtcNow, expireAt, nameof(expireAt));
        var record = await _runtimeTables.WorkflowSemaphore.ReadAsync(id.ToGuid(), cancellationToken);
        if (record == null)
        {
            throw new FulcrumNotFoundException($"Could not find a semaphore with id {id}.");
        }
        if (record.WorkflowInstanceId == workflowInstanceId.ToGuid())
        {
            FulcrumAssert.IsTrue(record.Raised, CodeLocation.AsString());
            record.ExpiresAt = expireAt;
            try
            {
                await _runtimeTables.WorkflowSemaphore.UpdateAsync(record.Id, record, cancellationToken);
                return;
            }
            catch (FulcrumConflictException)
            {
                throw new FulcrumTryAgainException(
                    $"Failed to extend the expiration time for the semaphore {record}");
            }
        }

        throw new FulcrumConflictException($"The semaphore {record} has been taken over by another workflow instance.");
    }

    /// <inheritdoc />
    public async Task<string> LowerAndReturnNextWorkflowInstanceAsync(string id, string workflowInstanceId, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
        InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
        var record = await _runtimeTables.WorkflowSemaphore.ReadAsync(id.ToGuid(), cancellationToken);
        if (record == null)
        {
            throw new FulcrumNotFoundException($"Could not find a semaphore with id {id}.");
        }
        if (record.WorkflowInstanceId == workflowInstanceId.ToGuid())
        {
            FulcrumAssert.IsTrue(record.Raised, CodeLocation.AsString());
            record.Raised = false;
            try
            {
                await _runtimeTables.WorkflowSemaphore.UpdateAsync(record.Id, record, cancellationToken);
            }
            catch (FulcrumConflictException)
            {
                throw new FulcrumTryAgainException(
                    $"Failed to lower the semaphore {record}");
            }
            return await NextWorkflowInstanceAsync(record, cancellationToken);
        }

        throw new FulcrumConflictException($"The semaphore {record} has been taken over by another workflow instance.");
    }

    private async Task<string> NextWorkflowInstanceAsync(WorkflowSemaphoreRecord record, CancellationToken cancellationToken)
    {
        var searchDetails = new SearchDetails<WorkflowSemaphoreQueueRecord>(new WorkflowSemaphoreQueueRecordSearch
        {
            WorkflowSemaphoreId = record.Id
        }, new
        {
            RecordCreatedAt = true
        });
        var queueItems = await _runtimeTables.WorkflowSemaphoreQueue.SearchAsync(searchDetails, 0, 1, cancellationToken);
        if (queueItems.PageInfo.Returned == 0) return null;
        FulcrumAssert.AreEqual(1, queueItems.PageInfo.Returned, CodeLocation.AsString());
        var queueItem = queueItems.Data.FirstOrDefault();
        FulcrumAssert.IsNotNull(queueItem, CodeLocation.AsString());
        return queueItem!.WorkflowInstanceId.ToGuidString();
    }
}