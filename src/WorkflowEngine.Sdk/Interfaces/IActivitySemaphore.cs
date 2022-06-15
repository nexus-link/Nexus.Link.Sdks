using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Semaphore"/>.
/// </summary>
[Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
public interface IActivitySemaphore : IActivity
{
    /// <summary>
    /// Raise a semaphore
    /// </summary>
    /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
    /// <param name="cancellationToken"></param>
    [Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    Task RaiseAsync(TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raise a semaphore with a <paramref name="limit"/> of the number of concurrent executions.
    /// </summary>
    /// <param name="limit">The maximum number of concurrent executions.</param>
    /// <param name="expiresAfter">How long time can we hold the semaphore?</param>
    /// <param name="cancellationToken"></param>
    [Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    Task RaiseWithLimitAsync(int limit, TimeSpan expiresAfter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lower a semaphore
    /// </summary>
    /// <param name="cancellationToken"></param>
    [Obsolete($"Please use {nameof(ActivityLock)} to lock within a workflow form and {nameof(ActivityThrottle)} to reduce the number of concurrent calls to a common resource (over all workflows). Obsolete since 2022-06-15.")]
    Task LowerAsync(CancellationToken cancellationToken = default);
}