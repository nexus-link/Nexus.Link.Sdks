using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivityLock" />
internal class ActivityLock : Activity, IActivityLock
{
    private readonly ISemaphoreSupport _semaphoreSupport;

    public ActivityLock(IActivityInformation activityInformation, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
        _semaphoreSupport = semaphoreSupport;
        _semaphoreSupport.Activity = this;
    }

    /// <inheritdoc />
    public string ResourceIdentifier => _semaphoreSupport.ResourceIdentifier;

    /// <inheritdoc />
    public Task ExecuteAsync(ActivityMethodAsync<IActivityLock> methodAsync, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => InternalExecuteAsync(methodAsync, ct), cancellationToken);
    }

    internal async Task InternalExecuteAsync(ActivityMethodAsync<IActivityLock> methodAsync, CancellationToken cancellationToken)
    {
        await _semaphoreSupport.RaiseAsync(cancellationToken);
        await methodAsync(this, cancellationToken);
        await _semaphoreSupport.LowerAsync(cancellationToken);
    }
}

internal class ActivityLock<TActivityReturns> : Activity<TActivityReturns>, IActivityLock<TActivityReturns>
{
    private readonly ISemaphoreSupport _semaphoreSupport;

    public ActivityLock(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
        _semaphoreSupport = semaphoreSupport;
        _semaphoreSupport.Activity = this;
    }

    /// <inheritdoc />
    public string ResourceIdentifier => _semaphoreSupport.ResourceIdentifier;

    /// <inheritdoc />
    public Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLock<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(ct => InternalExecuteAsync(methodAsync, ct), DefaultValueMethodAsync, cancellationToken);
    }

    private async Task<TActivityReturns> InternalExecuteAsync(ActivityMethodAsync<IActivityLock<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken)
    {
        await _semaphoreSupport.RaiseAsync(cancellationToken);
        var result = await methodAsync(this, cancellationToken);
        await _semaphoreSupport.LowerAsync(cancellationToken);
        return result;
    }
}