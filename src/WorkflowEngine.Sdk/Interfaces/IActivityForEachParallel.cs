using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for a parallel for each activity with no return value.
/// </summary>
/// <param name="item">The current individual item.</param>
/// <param name="activity">The current <see cref="IActivityForEachParallel{TItem}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TItem">The type for an individual item.</typeparam>
public delegate Task ActivityForEachParallelMethodAsync<in TItem>(TItem item, IActivityForEachParallel<TItem> activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for a parallel for each activity with a return value.
/// </summary>
/// <param name="item">The current individual item.</param>
/// <param name="activity">The current <see cref="IActivityForEachParallel{TMethodReturns,TItem}"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
/// <typeparam name="TItem">The type for an individual item.</typeparam>
public delegate Task<TMethodReturns> ActivityForEachParallelMethodAsync<TMethodReturns, in TItem>(TItem item, IActivityForEachParallel<TMethodReturns, TItem> activity, CancellationToken cancellationToken);

/// <summary>
/// Get the key for a specific item.
/// </summary>
/// <typeparam name="T">The type of the item</typeparam>
public delegate string GetKeyMethod<in T>(T item);

/// <summary>
/// Get the iteration title for a specific item.
/// </summary>
/// <typeparam name="T">The type of the item</typeparam>
public delegate string GetIterationTitleMethod<in T>(T item);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.ForEachParallel"/>.
/// </summary>
/// <typeparam name="TItem">The type for an individual item.</typeparam>
public interface IActivityForEachParallel<out TItem> : IExecutableActivity, ILoopActivity
{
    /// <summary>
    /// The items to loop over
    /// </summary>
    IEnumerable<TItem> Items { get; }

    /// <summary>
    /// Execute the <paramref name="methodAsync"/> for all items.
    /// </summary>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with Action(method). Obsolete since 2022-05-01.")]
    Task ExecuteAsync(ActivityForEachParallelMethodAsync<TItem> methodAsync, CancellationToken cancellationToken = default);
}

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.ForEachParallel"/>.
/// </summary>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
/// <typeparam name="TItem">The type for an individual item.</typeparam>
public interface IActivityForEachParallel<TMethodReturns, out TItem> : IExecutableActivity<IDictionary<string, TMethodReturns>>, ILoopActivity
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
    Task<IDictionary<string, TMethodReturns>> ExecuteAsync(ActivityForEachParallelMethodAsync<TMethodReturns, TItem> methodAsync, CancellationToken cancellationToken = default);
}