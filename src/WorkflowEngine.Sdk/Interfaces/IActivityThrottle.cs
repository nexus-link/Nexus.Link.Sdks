using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// The throttling is active over all workflow forms.
/// </summary>
public interface IActivityThrottle : IActivity
{
    /// <summary>
    /// The identifier of the resource that needs throttling
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// The maximum number of concurrent workflows over the resource.
    /// </summary>
    int Limit { get; }

    /// <summary>
    /// The time span that the <see cref="Limit"/> applies to. Null means that the <see cref="Limit"/> is for concurrent instances.
    /// </summary>
    /// <remarks>
    /// Must be null for locks.
    /// </remarks>
    TimeSpan? LimitationTimeSpan { get; }

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    /// <remarks>
    /// If the semaphore has a <see cref="LimitationTimeSpan"/>, then the semaphore is not lowered until the time has expired,
    /// otherwise the semaphore is always lowered after we tried to run the <paramref name="methodAsync"/>, even if there is an exception.
    /// </remarks>
    Task ExecuteAsync(ActivityMethodAsync<IActivityThrottle> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// </summary>
public interface IActivityThrottle<TActivityReturns> : IActivity
{
    /// <summary>
    /// The identifier of the resource that needs throttling
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// The maximum number of concurrent workflows over the resource.
    /// </summary>
    int Limit { get; }

    /// <summary>
    /// The time span that the <see cref="Limit"/> applies to. Null means that the <see cref="Limit"/> is for concurrent instances.
    /// </summary>
    /// <remarks>
    /// Must be null for locks.
    /// </remarks>
    TimeSpan? LimitationTimeSpan { get; }

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    /// <remarks>
    /// If the semaphore has a <see cref="LimitationTimeSpan"/>, then the semaphore is not lowered until the time has expired,
    /// otherwise the semaphore is always lowered after we tried to run the <paramref name="methodAsync"/>, even if there is an exception.
    /// </remarks>
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}