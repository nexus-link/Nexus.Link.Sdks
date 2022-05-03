using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for a sequential for each activity with no return value.
/// </summary>
/// <param name="item">The current individual item.</param>
/// <param name="activity">The current <see cref="IActivityForEachSequential{TItem}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TItem">The type for an individual item.</typeparam>
public delegate Task ActivityForEachSequentialMethodAsync<in TItem>(TItem item, IActivityForEachSequential<TItem> activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for a sequential for each activity with a return value.
/// </summary>
/// <param name="item">The current individual item.</param>
/// <param name="activity">The current <see cref="IActivityForEachSequential{TMethodReturns,TItem}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
/// <typeparam name="TItem">The type for an individual item.</typeparam>
public delegate Task<TMethodReturns> ActivityForEachSequentialMethodAsync<TMethodReturns, in TItem>(TItem item, IActivityForEachSequential<TMethodReturns, TItem> activity, CancellationToken cancellationToken);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.ForEachSequential"/>.
/// </summary>
public interface IActivityForEachSequential<out TItem> : IExecutableActivity, ILoopActivity
{
    /// <summary>
    /// The items to loop over
    /// </summary>
    IEnumerable<TItem> Items { get; }

    /// <summary>
    /// Execute the <paramref name="methodAsync"/> for all items.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task ExecuteAsync(ActivityForEachSequentialMethodAsync<TItem> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.ForEachSequential"/>.
/// </summary>
public interface IActivityForEachSequential<TMethodReturns, out TItem> : IExecutableActivity<IList<TMethodReturns>>, ILoopActivity
{
    /// <summary>
    /// The items to loop over
    /// </summary>
    IEnumerable<TItem> Items { get; }

    /// <summary>
    /// Execute the <paramref name="methodAsync"/> for all items.
    /// </summary>
    /// <returns>A dictionary that associates each item with a result</returns>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task<IList<TMethodReturns>> ExecuteAsync(ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync, CancellationToken cancellationToken = default);
}