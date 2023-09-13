using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Lock"/>, i.e. an activity that is executed while a resource is locked.
/// </summary>
public interface IActivityLock : IActivity
{
    /// <summary>
    /// The identifier of the resource we should lock
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="methodAsync"></param>
    IActivityLockThen Then(ActivityMethodAsync<IActivityLock> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="method"></param>
    IActivityLockThen Then(ActivityMethod<IActivityLock> method);

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    Task ExecuteAsync(ActivityMethodAsync<IActivityLock> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Lock"/>
/// </summary>
public interface IActivityLockThen : IExecutableActivity
{
    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="methodAsync"></param>
    IExecutableActivity Else(ActivityMethodAsync<IActivityLock> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="method"></param>
    IExecutableActivity Else(ActivityMethod<IActivityLock> method);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Lock"/>.
/// </summary>
public interface IActivityLock<TActivityReturns> : IActivity
{
    /// <summary>
    /// The identifier of the resource we should lock
    /// </summary>
    string ResourceIdentifier { get; }

    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="methodAsync"></param>
    IActivityLockThen<TActivityReturns> Then(ActivityMethodAsync<IActivityLock<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if Lock was successfully acquired.
    /// </summary>
    /// <param name="method"></param>
    IActivityLockThen<TActivityReturns> Then(ActivityMethod<IActivityLock<TActivityReturns>, TActivityReturns> method);

    /// <summary>
    /// Raise a semaphore with a maximum number of concurrent executions, execute the <paramref name="methodAsync"/>, lower the semaphore.
    /// </summary>
    [Obsolete($"Please use {nameof(Then)}. Obsolete since 2022-06-01")]
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLock<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Lock"/>
/// </summary>
/// <typeparam name="TActivityReturns"></typeparam>
public interface IActivityLockThen<TActivityReturns> : IExecutableActivity<TActivityReturns>
{
    /// <summary>
    /// Declare that <paramref name="methodAsync"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="methodAsync"></param>
    IExecutableActivity<TActivityReturns> Else(ActivityMethodAsync<IActivityLock<TActivityReturns>, TActivityReturns> methodAsync);

    /// <summary>
    /// Declare that <paramref name="method"/> should be executed if resource was already locked.
    /// </summary>
    /// <param name="method"></param>
    IExecutableActivity<TActivityReturns> Else(ActivityMethod<IActivityLock<TActivityReturns>, TActivityReturns> method);
}