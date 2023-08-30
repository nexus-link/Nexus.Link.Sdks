using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivityThrottle" />
[Obsolete($"Please use {nameof(IActivityAction)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
internal class ActivityThrottle : LockOrThrottleActivity<IActivityThrottle, IActivityThrottleThen>, IActivityThrottle, IActivityThrottleThen
{
    public ActivityThrottle(IActivityInformation activityInformation, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation, semaphoreSupport)
    {
        InternalContract.RequireNotNull(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.RequireValidated(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.Require(semaphoreSupport.IsThrottle, $"The parameter {semaphoreSupport} was supposed to have {nameof(semaphoreSupport.IsThrottle)} == true");
    }

    /// <inheritdoc />
    public int Limit => SemaphoreSupport.Limit;

    /// <inheritdoc />
    public TimeSpan? LimitationTimeSpan => SemaphoreSupport.LimitationTimeSpan;

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    public Task ExecuteAsync(ActivityMethodAsync<IActivityThrottle> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(LockOrThrottleAsync, cancellationToken);
    }
}

[Obsolete($"Please use {nameof(IActivityAction)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
internal class ActivityThrottle<TActivityReturns> : ActivityLockOrThrottle<TActivityReturns, IActivityThrottle<TActivityReturns>, IActivityThrottleThen<TActivityReturns>>, IActivityThrottle<TActivityReturns>, IActivityThrottleThen<TActivityReturns>
{
    public ActivityThrottle(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation, defaultValueMethodAsync, semaphoreSupport)
    {
        InternalContract.RequireNotNull(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.RequireValidated(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.Require(semaphoreSupport.IsThrottle, $"The parameter {semaphoreSupport} was supposed to have {nameof(semaphoreSupport.IsThrottle)} == true");
    }

    /// <inheritdoc />
    public int Limit => SemaphoreSupport.Limit;

    /// <inheritdoc />
    public TimeSpan? LimitationTimeSpan => SemaphoreSupport.LimitationTimeSpan;

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    public Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithReturnValueAsync(LockOrThrottleAsync, DefaultValueMethodAsync, cancellationToken);
    }
}