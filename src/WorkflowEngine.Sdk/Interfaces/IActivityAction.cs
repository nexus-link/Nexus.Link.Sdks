using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction : ITryCatchActivity
{
    /// <summary>
    /// Do the activity under a lock. This protects from other workflow instances of the same workflow form.
    /// If you want to protect from all other instances, no matter from which workflow form, then please use
    /// <see cref="WithThrottle"/>
    /// </summary>
    /// <param name="resourceIdentifier">The resource that should be locked, or null for a general lock.</param>
    /// <returns></returns>
    IActivityAction UnderLock(string resourceIdentifier);

    /// <summary>
    /// The activity uses a resource that needs throttling. Make sure that there is a limited of concurrent
    /// workflows that use this resource (for all workflow instances, no matter which workflow form they belong to).
    /// </summary>
    /// <param name="resourceIdentifier">The resource that needs throttling. Can not be null.</param>
    /// <param name="limit">The max number of instances that can use the resource at any given time.</param>
    /// <param name="limitationTimeSpan">The time span that the <paramref name="limit"/> applies to. Null means that the <paramref name="limit"/> is for concurrent instances.</param>
    /// <returns></returns>
    IActivityAction WithThrottle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan = null);

    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task ExecuteAsync(ActivityMethodAsync<IActivityAction> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Action"/>.
/// </summary>
public interface IActivityAction<TActivityReturns> : ITryCatchActivity<TActivityReturns>
{
    /// <summary>
    /// Do the activity under a lock. This protects from other workflow instances of the same workflow form.
    /// If you want to protect from all other instances, no matter from which workflow form, then please use
    /// <see cref="WithThrottle"/>
    /// </summary>
    /// <param name="resourceIdentifier">The resource that should be locked, or null for a general lock.</param>
    /// <returns></returns>
    IActivityAction<TActivityReturns> UnderLock(string resourceIdentifier);

    /// <summary>
    /// The activity uses a resource that needs throttling. Make sure that there is a limited of concurrent
    /// workflows that use this resource (for all workflow instances, no matter which workflow form they belong to).
    /// </summary>
    /// <param name="resourceIdentifier">The resource that needs throttling. Can not be null.</param>
    /// <param name="limit">The max number of instances that can use the resource at any given time.</param>
    /// <param name="limitationTimeSpan">The time span that the <paramref name="limit"/> applies to. Null means that the <paramref name="limit"/> is for concurrent instances.</param>
    /// <returns></returns>
    IActivityAction<TActivityReturns> WithThrottle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan = null);

    /// <summary>
    /// Execute the action <paramref name="methodAsync"/>.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default);
}