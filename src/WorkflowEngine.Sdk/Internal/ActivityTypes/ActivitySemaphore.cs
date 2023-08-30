using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivitySemaphore" />
[Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
internal class ActivitySemaphore : Activity, IActivitySemaphore
{
    private const string ContextSemaphoreHolderId = "SemaphoreHolderId";
    private static readonly Dictionary<string, Activity> RaisedActivitySemaphores = new();

    public string ResourceIdentifier { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="activityInformation"></param>
    /// <param name="resourceIdentifier">A string that uniquely identifies the resource that is protected by the semaphore.</param>
    [Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows).")]
    public ActivitySemaphore(IActivityInformation activityInformation, string resourceIdentifier)
        : base(activityInformation)
    {
        InternalContract.Require(ActivityInformation.Workflow.SemaphoreService != null, $"You must provide a {typeof(IWorkflowSemaphoreService)}.");
        ResourceIdentifier = resourceIdentifier;
        if (string.IsNullOrWhiteSpace(ResourceIdentifier)) ResourceIdentifier = "";
    }

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    public async Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => InternalRaiseWithLimitAsync(1, expiresAfter, ct), cancellationToken);
    }

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    public async Task RaiseWithLimitAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => InternalRaiseWithLimitAsync(limit, expiresAfter, ct), cancellationToken);
    }
    internal async Task InternalRaiseWithLimitAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireGreaterThanOrEqualTo(1, limit, nameof(limit));
        if (Instance.HasCompleted && Instance.State == ActivityStateEnum.Success)
        {
            var success = TryGetInternalContext<string>(ContextSemaphoreHolderId, out var semaphoreHolderId);
            if (!success)
            {
                // The semaphore has been lowered
                return;
            }

            await LogicExecutor.ExecuteWithoutReturnValueAsync(
                ct => ActivityInformation.Workflow.SemaphoreService.ExtendAsync(semaphoreHolderId, null, ct), "Extend",
                cancellationToken);
            lock (RaisedActivitySemaphores)
            {
                RaisedActivitySemaphores[CalculatedKey] = this;
            }
        }
        await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => RaiseAsync(limit, expiresAfter, ct), "Raise", cancellationToken);
    }

    private string CalculatedKey => $"{WorkflowInstanceId}.{ActivityInformation.Parent?.ActivityInstanceId}.{ResourceIdentifier}";

    private async Task RaiseAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken)
    {
        InternalContract.RequireGreaterThanOrEqualTo(1, limit, nameof(limit));
        var semaphoreCreate = new WorkflowSemaphoreCreate
        {
            WorkflowFormId = ActivityInformation.Workflow.FormId,
            WorkflowInstanceId = WorkflowInstanceId,
            ParentActivityInstanceId = ActivityInformation.Parent?.ActivityInstanceId,
            ParentIteration = ActivityInformation.Parent?.InternalIteration,
            ResourceIdentifier = ResourceIdentifier,
            Limit = limit,
            ExpirationTime = expiresAfter
        };
        var semaphoreHolderId = await ActivityInformation.Workflow.SemaphoreService.RaiseAsync(semaphoreCreate, cancellationToken);
        FulcrumAssert.IsNotNull(semaphoreHolderId, CodeLocation.AsString());
        lock (RaisedActivitySemaphores)
        {
            SetInternalContext(ContextSemaphoreHolderId, semaphoreHolderId);
            RaisedActivitySemaphores[CalculatedKey] = this;
        }
    }

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    public Task LowerAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(InternalLowerAsync, cancellationToken);
    }

    internal async Task InternalLowerAsync(CancellationToken cancellationToken = default)
    {
        string semaphoreHolderId;
        Activity raisedActivitySemaphore;
        lock (RaisedActivitySemaphores)
        {
            var found = RaisedActivitySemaphores.TryGetValue(CalculatedKey, out raisedActivitySemaphore);
            FulcrumAssert.IsTrue(found, CodeLocation.AsString());
            var success = raisedActivitySemaphore!.TryGetInternalContext(ContextSemaphoreHolderId, out semaphoreHolderId);
            FulcrumAssert.IsTrue(success, CodeLocation.AsString());
            FulcrumAssert.IsNotNullOrWhiteSpace(semaphoreHolderId, CodeLocation.AsString());
        }
        await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => ActivityInformation.Workflow.SemaphoreService.LowerAsync(semaphoreHolderId, ct), "Lower", cancellationToken);
        lock (RaisedActivitySemaphores)
        {
            raisedActivitySemaphore.RemoveInternalContext(ContextSemaphoreHolderId);
            RaisedActivitySemaphores.Remove(CalculatedKey);
        }
    }
}