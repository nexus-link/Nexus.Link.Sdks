using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivityThrottle" />
internal class ActivityThrottle : Activity, IActivityThrottle
{
    private readonly ISemaphoreSupport _semaphoreSupport;

    public ActivityThrottle(IActivityInformation activityInformation, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
        _semaphoreSupport = semaphoreSupport;
        _semaphoreSupport.Activity = this;
    }

    /// <inheritdoc />
    public string ResourceIdentifier => _semaphoreSupport.ResourceIdentifier;

    /// <inheritdoc />
    public int Limit => _semaphoreSupport.Limit;

    /// <inheritdoc />
    public TimeSpan? LimitationTimeSpan => _semaphoreSupport.LimitationTimeSpan;

    /// <inheritdoc />
    public Task ExecuteAsync(ActivityMethodAsync<IActivityThrottle> methodAsync, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => InternalExecuteAsync(methodAsync, ct), cancellationToken);
    }

    private async Task InternalExecuteAsync(ActivityMethodAsync<IActivityThrottle> methodAsync, CancellationToken cancellationToken)
    {
        await _semaphoreSupport.RaiseAsync(cancellationToken);
        try
        {
            await methodAsync(this, cancellationToken);
        }
        finally
        {
            await _semaphoreSupport.LowerAsync(cancellationToken);
        }
    }
}

internal class ActivityThrottle<TActivityReturns> : Activity<TActivityReturns>, IActivityThrottle<TActivityReturns>
{
    private readonly ISemaphoreSupport _semaphoreSupport;

    public ActivityThrottle(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
        _semaphoreSupport = semaphoreSupport;
        _semaphoreSupport.Activity = this;
    }

    /// <inheritdoc />
    public string ResourceIdentifier => _semaphoreSupport.ResourceIdentifier;

    /// <inheritdoc />
    public int Limit => _semaphoreSupport.Limit;

    /// <inheritdoc />
    public TimeSpan? LimitationTimeSpan => _semaphoreSupport.LimitationTimeSpan;

    /// <inheritdoc />
    public Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(ct => InternalExecuteAsync(methodAsync, ct), DefaultValueMethodAsync, cancellationToken);
    }

    private async Task<TActivityReturns> InternalExecuteAsync(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken)
    {
        await _semaphoreSupport.RaiseAsync(cancellationToken);
        var result = await methodAsync(this, cancellationToken);
        await _semaphoreSupport.LowerAsync(cancellationToken);
        return result;
    }
}