using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

internal class SemaphoreSupport : ISemaphoreSupport
{
    private readonly bool _isThrottle;
    public Activity Activity { get; set; }

    /// <inheritdoc />
    public string ResourceIdentifier { get; }

    /// <inheritdoc />
    public int Limit { get; }

    private string ContextKey => $"Semaphore-{ResourceIdentifier}";

    public SemaphoreSupport(string resourceIdentifier, int limit)
    {
        InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier));
        InternalContract.RequireGreaterThan(0, limit, nameof(limit));
        _isThrottle = true;
        ResourceIdentifier = resourceIdentifier;
        Limit = limit;
    }

    public SemaphoreSupport(string resourceIdentifier)
    {
        if (resourceIdentifier == null)
        {
            resourceIdentifier = "";
        }
        else
        {
            InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier),
                $"The parameter {nameof(resourceIdentifier)} must not be empty and not only contain whitespace.");
        }
        _isThrottle = false;
        ResourceIdentifier = resourceIdentifier;
        Limit = 1;
    }

    /// <inheritdoc />
    public async Task<string> RaiseAsync(CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Activity, CodeLocation.AsString());
        if (Activity.TryGetContext<string>(ContextKey, out var semaphoreHolderId))
        {
            await Activity.ActivityInformation.Workflow.SemaphoreService.ExtendAsync(semaphoreHolderId, null, cancellationToken);
        }
        else
        {
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = _isThrottle ? Guid.Empty.ToGuidString() : Activity.ActivityInformation.Workflow.FormId,
                WorkflowInstanceId = Activity.WorkflowInstanceId,
                ResourceIdentifier = ResourceIdentifier,
                Limit = Limit,
                ExpirationTime = TimeSpan.FromDays(365)
            };
            semaphoreHolderId =
                await Activity.ActivityInformation.Workflow.SemaphoreService.RaiseAsync(semaphoreCreate, cancellationToken);
            Activity.SetContext(ContextKey, semaphoreHolderId);
        }
        return semaphoreHolderId;
    }

    /// <inheritdoc />
    public async Task LowerAsync(CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Activity, CodeLocation.AsString());
        if (!Activity.TryGetContext<string>(ContextKey, out var semaphoreHolderId)) return;
        FulcrumAssert.IsNotNullOrWhiteSpace(semaphoreHolderId, CodeLocation.AsString());
        await Activity.ActivityInformation.Workflow.SemaphoreService.LowerAsync(semaphoreHolderId, cancellationToken);
    }
}
