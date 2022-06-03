using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivityThrottle" />
internal class ActivityThrottle : ActivityLockOrThrottle<IActivityThrottle, IActivityThrottleThen>, IActivityThrottle, IActivityThrottleThen
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
    public Task ExecuteAsync(ActivityMethodAsync<IActivityThrottle> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(LockOrThrottleAsync, cancellationToken);
    }
}

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
    public Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithReturnValueAsync(LockOrThrottleAsync, DefaultValueMethodAsync, cancellationToken);
    }
}