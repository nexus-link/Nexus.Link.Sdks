using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityActionMaybeFireAndForget : IActivityAction
{
    /// <summary>
    /// Do not wait for the response for this action, just hand it over to asynchronous execution.
    /// </summary>
    IExecutableActivity SetFireAndForget();
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction : ITryCatchActivity, IActivityActionCanHaveLock
{
    /// <summary>
    /// Set the maximum execution time to the absolute time <paramref name="time"/>.
    /// </summary>
    /// <param name="time"></param>
    IActivityAction SetMaxTime(DateTimeOffset time);

    /// <summary>
    /// Set the maximum execution time to the time activity started originally + <paramref name="timeSpan"/>.
    /// </summary>
    /// <param name="timeSpan"></param>
    IActivityAction SetMaxTime(TimeSpan timeSpan);

    /// <summary>
    /// If the method sends an HTTP request, we will first try to send the request synchronously.
    /// If the request does not return a HTTP status code = 200, then we will try to do it asynchronously.
    /// </summary>
    /// <returns></returns>
    IActivityAction TrySynchronousHttpRequestFirst();

    /// <summary>
    /// The activity uses a resource that needs throttling. Make sure that there is a limited of concurrent
    /// workflows that use this resource (for all workflow instances, no matter which workflow form they belong to).
    /// </summary>
    /// <param name="resourceIdentifier">The resource that needs throttling. Can not be null.</param>
    /// <param name="limit">The max number of instances that can use the resource at any given time.</param>
    /// <param name="limitationTimeSpan">The time span that the <paramref name="limit"/> applies to. Null means that the <paramref name="limit"/> is for concurrent instances.</param>
    /// <returns></returns>
    IActivityActionWithThrottle WithThrottle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan = null);

    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task ExecuteAsync(ActivityMethodAsync<IActivityAction> methodAsync, CancellationToken cancellationToken = default);
}



/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityActionWithThrottle : IActivityActionCanHaveLock, IActivityActionLockOrThrottle
{
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityActionCanHaveLock
{

    /// <summary>
    /// Do the activity under a lock. This protects from other workflow instances of the same workflow form.
    /// If you want to protect from all other instances, no matter from which workflow form, then please use
    /// <see cref="IActivityAction.WithThrottle"/>
    /// </summary>
    /// <param name="resourceIdentifier">The resource that should be locked, or null for a general lock.</param>
    /// <returns></returns>
    IActivityActionLockOrThrottle UnderLock(string resourceIdentifier = null);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityActionLockOrThrottle : ITryCatchActivity
{
    /// <summary>
    /// If the action is under a lock or with throttling, then <paramref name="whenWaitingAsync"/> is called only if
    /// we are waiting for the resource to be available, i.e. it is already fully utilized by other workflow instances.
    /// </summary>
    ITryCatchActivity WhenWaiting(ActivityMethodAsync<IActivityAction> whenWaitingAsync);

    /// <summary>
    /// If the action is under a lock or with throttling, then <paramref name="whenWaiting"/> is called only if
    /// we are waiting for the resource to be available, i.e. it is already fully utilized by other workflow instances.
    /// </summary>
    ITryCatchActivity WhenWaiting(ActivityMethod<IActivityAction> whenWaiting);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction<TActivityReturns> : ITryCatchActivity<TActivityReturns>, IActivityActionCanHaveLock<TActivityReturns>
{
    /// <summary>
    /// Set the maximum execution time to the absolute time <paramref name="time"/>.
    /// </summary>
    /// <param name="time"></param>
    IActivityAction<TActivityReturns> SetMaxTime(DateTimeOffset time);

    /// <summary>
    /// Set the maximum execution time to the time activity started originally + <paramref name="timeSpan"/>.
    /// </summary>
    /// <param name="timeSpan"></param>
    IActivityAction<TActivityReturns> SetMaxTime(TimeSpan timeSpan);

    /// <summary>
    /// If the method sends an HTTP request, we will first try to send the request synchronously.
    /// If the request does not return a HTTP status code = 200, then we will try to do it asynchronously.
    /// </summary>
    /// <returns></returns>
    IActivityAction<TActivityReturns> TrySynchronousHttpRequestFirst();

    /// <summary>
    /// The activity uses a resource that needs throttling. Make sure that there is a limited of concurrent
    /// workflows that use this resource (for all workflow instances, no matter which workflow form they belong to).
    /// </summary>
    /// <param name="resourceIdentifier">The resource that needs throttling. Can not be null.</param>
    /// <param name="limit">The max number of instances that can use the resource at any given time.</param>
    /// <param name="limitationTimeSpan">The time span that the <paramref name="limit"/> applies to. Null means that the <paramref name="limit"/> is for concurrent instances.</param>
    /// <returns></returns>
    IActivityActionWithThrottle<TActivityReturns> WithThrottle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan = null);

    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityActionWithThrottle<TActivityReturns> : IActivityActionCanHaveLock<TActivityReturns>, IActivityActionLockOrThrottle<TActivityReturns>
{
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityActionCanHaveLock<TActivityReturns>
{
    /// <summary>
    /// Do the activity under a lock. This protects from other workflow instances of the same workflow form.
    /// If you want to protect from all other instances, no matter from which workflow form, then please use
    /// <see cref="IActivityAction{T}.WithThrottle"/>
    /// </summary>
    /// <param name="resourceIdentifier">The resource that should be locked, or null for a general lock.</param>
    /// <returns></returns>
    IActivityActionLockOrThrottle<TActivityReturns> UnderLock(string resourceIdentifier);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.If"/>.
/// </summary>
public interface IActivityActionLockOrThrottle<TActivityReturns> : ITryCatchActivity<TActivityReturns>
{
    /// <summary>
    /// If the action is under a lock or with throttling, then <paramref name="whenWaitingAsync"/> is called only if
    /// we are waiting for the resource to be available, i.e. it is already fully utilized by other workflow instances.
    /// </summary>
    ITryCatchActivity<TActivityReturns> WhenWaiting(ActivityMethodAsync<IActivityAction<TActivityReturns>> whenWaitingAsync);

    /// <summary>
    /// If the action is under a lock or with throttling, then <paramref name="whenWaiting"/> is called only if
    /// we are waiting for the resource to be available, i.e. it is already fully utilized by other workflow instances.
    /// </summary>
    ITryCatchActivity<TActivityReturns> WhenWaiting(ActivityMethod<IActivityAction<TActivityReturns>> whenWaiting);
}