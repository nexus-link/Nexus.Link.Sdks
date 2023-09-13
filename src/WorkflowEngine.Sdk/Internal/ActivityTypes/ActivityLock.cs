using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivityLock" />
internal class ActivityLock : LockOrThrottleActivity<IActivityLock, IActivityLockThen>, IActivityLock, IActivityLockThen
{
    public ActivityLock(IActivityInformation activityInformation, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation, semaphoreSupport)
    {
        InternalContract.RequireNotNull(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.RequireValidated(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.Require(!semaphoreSupport.IsThrottle, $"The parameter {semaphoreSupport} was supposed to have {nameof(semaphoreSupport.IsThrottle)} == false");
        InternalContract.Require(semaphoreSupport.Limit == 1, $"The parameter {semaphoreSupport} was supposed to have {nameof(semaphoreSupport.Limit)} == 1");
    }

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    public Task ExecuteAsync(ActivityMethodAsync<IActivityLock> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(LockOrThrottleAsync, cancellationToken);
    }
}

internal class ActivityLock<TActivityReturns> : ActivityLockOrThrottle<TActivityReturns, IActivityLock<TActivityReturns>, IActivityLockThen<TActivityReturns>>, IActivityLock<TActivityReturns>, IActivityLockThen<TActivityReturns>
{
    public ActivityLock(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation, defaultValueMethodAsync, semaphoreSupport)
    {
        InternalContract.RequireNotNull(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.RequireValidated(semaphoreSupport, nameof(semaphoreSupport));
        InternalContract.Require(!semaphoreSupport.IsThrottle, $"The parameter {semaphoreSupport} was supposed to have {nameof(semaphoreSupport.IsThrottle)} == false");
    }

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    public Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLock<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithReturnValueAsync(LockOrThrottleAsync, DefaultValueMethodAsync, cancellationToken);
    }
}