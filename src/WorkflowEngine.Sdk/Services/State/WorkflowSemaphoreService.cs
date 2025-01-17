using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Log = Nexus.Link.Libraries.Core.Logging.Log;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State;

/// <inheritdoc />
public class WorkflowSemaphoreService : IWorkflowSemaphoreService
{
    private readonly IAsyncRequestMgmtCapability _requestMgmtCapability;
    private readonly IRuntimeTables _runtimeTables;

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkflowSemaphoreService(IAsyncRequestMgmtCapability requestMgmtCapability, IRuntimeTables runtimeTables)
    {
        _requestMgmtCapability = requestMgmtCapability;
        _runtimeTables = runtimeTables;
    }

    /// <inheritdoc />
    public async Task<string> RaiseAsync(WorkflowSemaphoreCreate item, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNull(item, nameof(item));
        InternalContract.RequireValidated(item, nameof(item));

        try
        {
            var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            using var scope = new TransactionScope(TransactionScopeOption.Required, options,
                TransactionScopeAsyncFlowOption.Enabled);

            var (semaphore, holder) = await GetOrCreateSemaphoreAndHolderWithLockAsync(item, cancellationToken);
            if (holder.Raised)
            {
                // Already raised, extend the expiration time
                holder.ExpiresAt = DateTimeOffset.UtcNow + item.ExpirationTime;
                await _runtimeTables.WorkflowSemaphoreQueue.UpdateAsync(holder.Id, holder, cancellationToken);
            }
            else
            {
                var raisedHolder = await ActivateItemsInQueueAsync(semaphore.Id, item.Limit, holder, cancellationToken);

                if (raisedHolder == null)
                {
                    scope.Complete();
                    throw new ActivityTemporaryErrorException(TimeSpan.FromHours(1));
                }

                if (semaphore.Limit != item.Limit)
                {
                    semaphore.Limit = item.Limit;
                    await _runtimeTables.WorkflowSemaphore.UpdateAsync(semaphore.Id, semaphore, cancellationToken);
                }
            }

            scope.Complete();
            return holder.Id.ToGuidString();
        }
        catch (Exception ex)
        {
            if (ex is RequestPostponedException or FulcrumTryAgainException) throw;
            throw new FulcrumTryAgainException($"{ex.GetType().Name}: {ex.Message}")
            {
                RecommendedWaitTimeInSeconds = TimeSpan.FromMinutes(5).TotalSeconds
            };
        }
    }

    /// <inheritdoc />
    public async Task ExtendAsync(string semaphoreHolderId, TimeSpan? timeToExpiration, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNullOrWhiteSpace(semaphoreHolderId, nameof(semaphoreHolderId));

        try
        {
            var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            using var scope = new TransactionScope(TransactionScopeOption.Required, options,
                TransactionScopeAsyncFlowOption.Enabled);

            var holder =
                await _runtimeTables.WorkflowSemaphoreQueue.ReadAsync(semaphoreHolderId.ToGuid(), cancellationToken);
            FulcrumAssert.IsNotNull(holder, CodeLocation.AsString());
            if (!holder.Raised)
            {
                throw new FulcrumConflictException($"The semaphore {holder} has lost the semaphore.");
            }

            // Already raised, extend the expiration time
            timeToExpiration ??= TimeSpan.FromSeconds(holder.ExpirationAfterSeconds);
            await _runtimeTables.WorkflowSemaphore.ClaimTransactionLockAsync(holder.WorkflowSemaphoreId,
                cancellationToken);
            holder.ExpiresAt = DateTimeOffset.UtcNow.Add(timeToExpiration.Value);
            await _runtimeTables.WorkflowSemaphoreQueue.UpdateAsync(holder.Id, holder, cancellationToken);
            scope.Complete();
        }
        catch (Exception ex)
        {
            if (ex is RequestPostponedException or FulcrumTryAgainException) throw;
            throw new FulcrumTryAgainException($"{ex.GetType().Name}: {ex.Message}")
            {
                RecommendedWaitTimeInSeconds = TimeSpan.FromMinutes(5).TotalSeconds
            };
        }
    }

    /// <inheritdoc />
    public async Task LowerAsync(string semaphoreHolderId, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNullOrWhiteSpace(semaphoreHolderId, nameof(semaphoreHolderId));

        var holder = await _runtimeTables.WorkflowSemaphoreQueue.ReadAsync(semaphoreHolderId.ToGuid(), cancellationToken);
        if (holder?.Raised != true) return;

        try
        {
            var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            using var scope = new TransactionScope(TransactionScopeOption.Required, options,
                TransactionScopeAsyncFlowOption.Enabled);
            var semaphore =
                await _runtimeTables.WorkflowSemaphore.ClaimTransactionLockAndReadAsync(holder.WorkflowSemaphoreId,
                    cancellationToken);
            await _runtimeTables.WorkflowSemaphoreQueue.DeleteAsync(holder.Id, cancellationToken);
            await ActivateItemsInQueueAsync(semaphore.Id, semaphore.Limit, null, cancellationToken);
            scope.Complete();
        }
        catch (Exception ex)
        {
            if (ex is RequestPostponedException or FulcrumTryAgainException) throw;
            throw new FulcrumTryAgainException($"{ex.GetType().Name}: {ex.Message}")
            {
                RecommendedWaitTimeInSeconds = TimeSpan.FromMinutes(5).TotalSeconds
            };
        }
    }

    /// <inheritdoc />
    public async Task LowerAllAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        // Set a time limit for this operation
        var limitedTimeCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limitedTimeCancellationToken.Token);

        var searchDetails = new SearchDetails<WorkflowSemaphoreQueueRecord>(new
        {
            WorkflowInstanceId = workflowInstanceId.ToGuidString()
        });
        var handlers = (await StorageHelper.ReadPagesAsync((o, ct) => _runtimeTables.WorkflowSemaphoreQueue.SearchAsync(searchDetails, o, null, ct), int.MaxValue, cancellationToken))
            .ToDictionary(record => record.Id);

        var queue = new Queue<Guid>(handlers.Keys);
        while (queue.TryDequeue(out var key))
        {
            var handler = handlers[key];
            try
            {
                mergedToken.Token.ThrowIfCancellationRequested();
                await LowerAsync(handler.Id.ToGuidString(), mergedToken.Token);
            }
            catch (FulcrumTryAgainException)
            {
                var sleep = !queue.Any();
                // The semaphore was locked by another process, enqueue to try later
                queue.Enqueue(key);
                if (sleep)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10), mergedToken.Token);
                }
            }
        }
    }

    /// <summary>
    /// Try to activate items in the queue
    /// </summary>
    /// <param name="semaphoreId">The semaphore queue that we should activate items for</param>
    /// <param name="limit">The limit for concurrent active items.</param>
    /// <param name="myHolder">If you are especially interested in one queue item, then set this.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>If you specified <paramref name="myHolder"/> and it has been activated, it is returned (with updated fields).</returns>
    private async Task<WorkflowSemaphoreQueueRecord> ActivateItemsInQueueAsync(
        Guid semaphoreId,
        int limit,
        WorkflowSemaphoreQueueRecord myHolder,
        CancellationToken cancellationToken)
    {
        WorkflowSemaphoreQueueRecord myHolderAsActivated = null;
        var holders = await GetHoldersAsync(semaphoreId, cancellationToken);
        var raisedHolders = holders
            .Where(h => h.Raised)
            .ToArray();
        var now = DateTimeOffset.UtcNow;
        var expiredHolders = raisedHolders
            .Where(h => h.ExpiresAt < now)
            .ToArray();
        var waitingHolders = holders
            .Where(h => !h.Raised)
            .OrderBy(h => h.RecordCreatedAt)
            .ToArray();
        var expireIndex = 0;
        var waitingIndex = 0;
        var raisedHoldersCount = raisedHolders.Length;

        while (waitingIndex < waitingHolders.Length && (raisedHoldersCount < limit || expireIndex < expiredHolders.Length))
        {
            // As long as we are under the limit, we can activate waiting holders
            while (raisedHoldersCount < limit && waitingIndex < waitingHolders.Length)
            {
                await ActivateHolderAsync(waitingHolders[waitingIndex++]);
                raisedHoldersCount++;
            }

            // As long as we have waiting items and no free slots and expired holders, remove expired holders
            while (waitingIndex < waitingHolders.Length && raisedHoldersCount >= limit && expireIndex < expiredHolders.Length)
            {
                await DeleteHolderAsync(expiredHolders[expireIndex++]);
                raisedHoldersCount--;
            }
        }

        return myHolderAsActivated;

        async Task ActivateHolderAsync(WorkflowSemaphoreQueueRecord waitingHolder)
        {
            waitingHolder.Raised = true;
            waitingHolder.ExpiresAt = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(waitingHolder.ExpirationAfterSeconds));
            var activatedHolder = await _runtimeTables.WorkflowSemaphoreQueue.UpdateAndReturnAsync(waitingHolder.Id, waitingHolder, cancellationToken);
            if (activatedHolder.WorkflowInstanceId == myHolder?.WorkflowInstanceId
                && activatedHolder.ParentActivityInstanceId == myHolder?.ParentActivityInstanceId
                && activatedHolder.ParentIteration == myHolder?.ParentIteration)
            {
                myHolderAsActivated = activatedHolder;
            }
            else
            {
                try
                {
                    await WorkflowHelper.RetryAsync(async () =>
                        await _requestMgmtCapability.Request.RetryAsync(activatedHolder.WorkflowInstanceId.ToGuidString(), cancellationToken),
                        3,
                        TimeSpan.FromMilliseconds(100));
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"Due to repeated exceptions, giving up on calling AsyncManager Request {nameof(_requestMgmtCapability.Request.RetryAsync)} " +
                        $"Error occured during Workflow execution for instance: {activatedHolder.WorkflowInstanceId.ToGuidString()}", ex);
                }
            }
        }

        async Task DeleteHolderAsync(WorkflowSemaphoreQueueRecord expiredHolder)
        {
            await _runtimeTables.WorkflowSemaphoreQueue.DeleteAsync(expiredHolder.Id, cancellationToken);
        }
    }

    private async Task<WorkflowSemaphoreQueueRecord[]> GetHoldersAsync(Guid semaphoreId, CancellationToken cancellationToken)
    {
        var where = new
        {
            WorkflowSemaphoreId = semaphoreId
        };
        var searchDetails = new SearchDetails<WorkflowSemaphoreQueueRecord>(@where);
        return (await StorageHelper.ReadPagesAsync((o, ct) =>
                    _runtimeTables.WorkflowSemaphoreQueue.SearchAsync(searchDetails, o, null, ct),
                int.MaxValue,
                cancellationToken))
            .ToArray();
    }

    private async Task<(WorkflowSemaphoreRecord semaphore, WorkflowSemaphoreQueueRecord holder)> GetOrCreateSemaphoreAndHolderWithLockAsync(WorkflowSemaphoreCreate item, CancellationToken cancellationToken)
    {
        var semaphoreCreate = new WorkflowSemaphoreRecordCreate().From(item);
        var semaphore = await GetOrCreateSemaphoreRecordAndLockAsync(semaphoreCreate, cancellationToken);
        FulcrumAssert.IsNotNull(semaphore, CodeLocation.AsString());
        var holderCreate = new WorkflowSemaphoreQueueRecordCreate().From(semaphore, item);
        var holder = await GetOrCreateSemaphoreQueueRecordAsync(holderCreate, cancellationToken);
        return (semaphore, holder);

    }

    private async Task<WorkflowSemaphoreQueueRecord> GetOrCreateSemaphoreQueueRecordAsync(
        WorkflowSemaphoreQueueRecordCreate recordCreate, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNull(recordCreate, nameof(recordCreate));
        InternalContract.RequireValidated(recordCreate, nameof(recordCreate));

        var count = 0;
        while (count < 3)
        {
            count++;
            var unique = new WorkflowSemaphoreQueueRecordUnique
            {
                WorkflowSemaphoreId = recordCreate.WorkflowSemaphoreId,
                WorkflowInstanceId = recordCreate.WorkflowInstanceId,
                ParentActivityInstanceId = recordCreate.ParentActivityInstanceId,
                ParentIteration = recordCreate.ParentIteration
            };
            var searchDetails = new SearchDetails<WorkflowSemaphoreQueueRecord>(unique);
            var holder = await _runtimeTables.WorkflowSemaphoreQueue.FindUniqueAsync(searchDetails, cancellationToken);
            if (holder != null) return holder;

            // No semaphore queue item found, this is a good opportunity to create it
            try
            {
                holder = await _runtimeTables.WorkflowSemaphoreQueue.CreateAndReturnAsync(recordCreate, cancellationToken);
                return holder;
            }
            catch (FulcrumConflictException)
            {
                // The queue record already exists, try again
            }
        }
        throw new FulcrumResourceLockedException($"Could not get or create a semaphore queue item ({recordCreate}) in three retries, due to racing conditions.");
    }

    private async Task<WorkflowSemaphoreRecord> GetOrCreateSemaphoreRecordAndLockAsync(
        WorkflowSemaphoreRecordCreate recordCreate, CancellationToken cancellationToken)
    {
        var count = 0;
        Guid? semaphoreId = null;
        while (count < 3)
        {
            count++;
            var unique = new WorkflowSemaphoreRecordUnique
            {
                WorkflowFormId = recordCreate.WorkflowFormId,
                ResourceIdentifier = recordCreate.ResourceIdentifier
            };
            var searchDetails = new SearchDetails<WorkflowSemaphoreRecord>(unique);
            var semaphore = await _runtimeTables.WorkflowSemaphore.FindUniqueAsync(searchDetails, cancellationToken);
            if (semaphore != null)
            {
                semaphoreId = semaphore.Id;
                break;
            }
            // No semaphore found, this is a good opportunity to create it
            try
            {
                semaphoreId = await _runtimeTables.WorkflowSemaphore.CreateAsync(recordCreate, cancellationToken);
                break;
            }
            catch (FulcrumConflictException)
            {
                // The semaphore record already exists, try again to read it
            }
        }

        if (semaphoreId.HasValue)
        {
            return await _runtimeTables.WorkflowSemaphore.ClaimTransactionLockAndReadAsync(semaphoreId.Value,
                cancellationToken);
        }

        throw new FulcrumResourceLockedException($"Could not get or create a semaphore {recordCreate} in three retries, due to racing conditions.");
    }
}