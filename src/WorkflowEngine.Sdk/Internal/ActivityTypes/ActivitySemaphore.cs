using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivitySemaphore" />
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
    public ActivitySemaphore(IActivityInformation activityInformation, string resourceIdentifier)
        : base(activityInformation)
    {
        InternalContract.Require(ActivityInformation.Workflow.SemaphoreService != null, $"You must provide a {typeof(IWorkflowSemaphoreService)}.");
        ResourceIdentifier = resourceIdentifier;
        if (string.IsNullOrWhiteSpace(ResourceIdentifier)) ResourceIdentifier = "";
    }

    /// <inheritdoc />
    public Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default)
    {
        return RaiseWithLimitAsync(1, expiresAfter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RaiseWithLimitAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireGreaterThanOrEqualTo(1, limit, nameof(limit));
        if (Instance.HasCompleted && Instance.State == ActivityStateEnum.Success)
        {
            var success = TryGetContext<string>(ContextSemaphoreHolderId, out var semaphoreHolderId);
            if (!success)
            {
                // The semaphore has been lowered
                return;
            }
            await ActivityInformation.Workflow.SemaphoreService.ExtendAsync(semaphoreHolderId, null, cancellationToken);
            lock (RaisedActivitySemaphores)
            {
                RaisedActivitySemaphores[CalculatedKey] = this;
            }
        }
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(
            ct => InternalRaiseAsync(limit, expiresAfter, ct),
            cancellationToken);
    }

    private string CalculatedKey => $"{WorkflowInstanceId}.{ActivityInformation.Parent?.ActivityInstanceId}.{ResourceIdentifier}";

    private async Task InternalRaiseAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken)
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
        lock (RaisedActivitySemaphores)
        {
            SetContext(ContextSemaphoreHolderId, semaphoreHolderId);
            RaisedActivitySemaphores[CalculatedKey] = this;
        }
    }

    /// <inheritdoc />
    public Task LowerAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(InternalLowerAsync, cancellationToken);
    }

    private Task InternalLowerAsync(CancellationToken cancellationToken)
    {
        string semaphoreHolderId;
        lock (RaisedActivitySemaphores)
        {
            var found = RaisedActivitySemaphores.TryGetValue(CalculatedKey, out var raisedActivitySemaphore);
            FulcrumAssert.IsTrue(found, CodeLocation.AsString());
            var success = raisedActivitySemaphore!.TryGetContext(ContextSemaphoreHolderId, out semaphoreHolderId);
            FulcrumAssert.IsTrue(success, CodeLocation.AsString());
            FulcrumAssert.IsNotNullOrWhiteSpace(semaphoreHolderId, CodeLocation.AsString());
            raisedActivitySemaphore.RemoveContext(ContextSemaphoreHolderId);
            RaisedActivitySemaphores.Remove(CalculatedKey);
        }
        return ActivityInformation.Workflow.SemaphoreService.LowerAsync(semaphoreHolderId, cancellationToken);
    }
}