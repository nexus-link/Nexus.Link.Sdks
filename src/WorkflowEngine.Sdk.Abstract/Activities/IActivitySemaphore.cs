using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Semaphore"/>.
/// </summary>
[Obsolete($"Please use {nameof(IActivityLock)} to lock within a workflow form and {nameof(IActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
public interface IActivitySemaphore : IActivity
{
    /// <summary>
    /// Raise a semaphore
    /// </summary>
    /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
    /// <param name="cancellationToken"></param>
    [Obsolete($"Please use {nameof(IActivityLock)} to lock within a workflow form and {nameof(IActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raise a semaphore with a <paramref name="limit"/> of the number of concurrent executions.
    /// </summary>
    /// <param name="limit">The maximum number of concurrent executions.</param>
    /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
    /// <param name="cancellationToken"></param>
    [Obsolete($"Please use {nameof(IActivityLock)}  to lock within a workflow form and  {nameof(IActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    Task RaiseWithLimitAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lower a semaphore
    /// </summary>
    /// <param name="cancellationToken"></param>
    [Obsolete($"Please use {nameof(IActivityLock)}  to lock within a workflow form and  {nameof(IActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    Task LowerAsync(CancellationToken cancellationToken = default);
}