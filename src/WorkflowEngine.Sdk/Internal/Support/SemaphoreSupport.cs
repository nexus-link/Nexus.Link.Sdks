﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

internal class SemaphoreSupport : ISemaphoreSupport
{

    /// <inheritdoc />
    public Activity Activity { get; set; }

    /// <inheritdoc />
    public bool IsThrottle { get; }

    /// <inheritdoc />
    public string ResourceIdentifier { get; }

    /// <inheritdoc />
    public int Limit { get; }

    /// <inheritdoc />
    public TimeSpan? LimitationTimeSpan { get; }

    private string ContextKey => $"Semaphore-{ResourceIdentifier}";

    public SemaphoreSupport(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan)
    {
        InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier));
        InternalContract.RequireGreaterThan(0, limit, nameof(limit));
        if (limitationTimeSpan.HasValue)
        {
            InternalContract.RequireGreaterThan(TimeSpan.FromMinutes(1), limitationTimeSpan.Value, nameof(limitationTimeSpan));
        }
        IsThrottle = true;
        ResourceIdentifier = resourceIdentifier;
        Limit = limit;
        LimitationTimeSpan = limitationTimeSpan;
    }

    public SemaphoreSupport(string resourceIdentifier = null)
    {
        if (string.IsNullOrWhiteSpace(resourceIdentifier))
        {
            resourceIdentifier = "*";
        }
        IsThrottle = false;
        ResourceIdentifier = resourceIdentifier;
        Limit = 1;
        LimitationTimeSpan = null;
    }

    /// <inheritdoc />
    public async Task<string> RaiseAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(Activity, CodeLocation.AsString());
        if (Activity.TryGetInternalContext<string>(ContextKey, out var semaphoreHolderId))
        {
            await Activity.ActivityInformation.Workflow.SemaphoreService.ExtendAsync(semaphoreHolderId, null, cancellationToken);
        }
        else
        {
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = IsThrottle ? null : Activity.ActivityInformation.Workflow.FormId,
                WorkflowInstanceId = Activity.WorkflowInstanceId,
                ParentActivityInstanceId = Activity.ActivityInformation.Parent?.ActivityInstanceId,
                ParentIteration = Activity.ActivityInformation.Parent?.InternalIteration,
                ResourceIdentifier = ResourceIdentifier,
                Limit = Limit,
                ExpirationTime = IsThrottle 
                    ? LimitationTimeSpan ?? TimeSpan.FromMinutes(2) // For throttling with no LimitationTimeSpan we will release it as soon as the workflow is paused and that is never later than 100 seconds into the workflow execution, du to IIS limitations.
                    : TimeSpan.FromDays(365)
            };
            semaphoreHolderId =
                await Activity.ActivityInformation.Workflow.SemaphoreService.RaiseAsync(semaphoreCreate, cancellationToken);
            Activity.SetInternalContext(ContextKey, semaphoreHolderId);
        }
        return semaphoreHolderId;
    }

    /// <inheritdoc />
    public async Task LowerAsync(CancellationToken cancellationToken)
    {
        if (LimitationTimeSpan.HasValue)
        {
            // We will let the semaphore time out by itself
            return;
        }
        FulcrumAssert.IsNotNull(Activity, CodeLocation.AsString());
        if (!Activity.TryGetInternalContext<string>(ContextKey, out var semaphoreHolderId)) return;
        FulcrumAssert.IsNotNullOrWhiteSpace(semaphoreHolderId, CodeLocation.AsString());
        await Activity.ActivityInformation.Workflow.SemaphoreService.LowerAsync(semaphoreHolderId, cancellationToken);
    }
}
