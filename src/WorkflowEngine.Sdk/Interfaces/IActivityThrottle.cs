using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// The throttling is active over all workflow forms.
/// </summary>
[Obsolete($"Please use {nameof(IActivityAction)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
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
    /// Declare that <paramref name="methodAsync"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="methodAsync"></param>
    IActivityThrottleThen Then(ActivityMethodAsync<IActivityThrottle> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="method"></param>
    IActivityThrottleThen Then(ActivityMethod<IActivityThrottle> method);

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    /// <remarks>
    /// If the semaphore has a <see cref="LimitationTimeSpan"/>, then the semaphore is not lowered until the time has expired,
    /// otherwise the semaphore is always lowered after we tried to run the <paramref name="methodAsync"/>, even if there is an exception.
    /// </remarks>
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    Task ExecuteAsync(ActivityMethodAsync<IActivityThrottle> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// The throttling is active over all workflow forms.
/// </summary>
[Obsolete($"Please use {nameof(IActivityAction)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
public interface IActivityThrottleThen : IExecutableActivity
{
    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="methodAsync"></param>
    IExecutableActivity Else(ActivityMethodAsync<IActivityThrottle> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="method"></param>
    IExecutableActivity Else(ActivityMethod<IActivityThrottle> method);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// </summary>
[Obsolete($"Please use {nameof(IActivityAction)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
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
    /// Declare that <paramref name="methodAsync"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="methodAsync"></param>
    IActivityThrottleThen<TActivityReturns> Then(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="method"></param>
    IActivityThrottleThen<TActivityReturns> Then(ActivityMethod<IActivityThrottle<TActivityReturns>, TActivityReturns> method);

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    /// <remarks>
    /// If the semaphore has a <see cref="LimitationTimeSpan"/>, then the semaphore is not lowered until the time has expired,
    /// otherwise the semaphore is always lowered after we tried to run the <paramref name="methodAsync"/>, even if there is an exception.
    /// </remarks>
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Throttle"/>, i.e. an activity where we try to reduce the number of workflow instances that access the same resource.
/// The throttling is active over all workflow forms.
/// </summary>
[Obsolete($"Please use {nameof(IActivityAction)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
public interface IActivityThrottleThen<TActivityReturns> : IExecutableActivity<TActivityReturns>
{
    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="methodAsync"></param>
    IExecutableActivity<TActivityReturns> Else(ActivityMethodAsync<IActivityThrottle<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="method"></param>
    IExecutableActivity<TActivityReturns> Else(ActivityMethod<IActivityThrottle<TActivityReturns>, TActivityReturns> method);
}