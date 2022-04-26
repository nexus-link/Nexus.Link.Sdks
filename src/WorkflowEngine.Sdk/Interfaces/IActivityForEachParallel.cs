using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.ForEachParallel"/>.
    /// </summary>
    public interface IActivityForEachParallel<out TItem> : IActivity
    {
        /// <summary>
        /// The items to loop over
        /// </summary>
        IEnumerable<TItem> Items { get; }

        /// <summary>
        /// Execute the <paramref name="method"/> for all items.
        /// </summary>
        Task ExecuteAsync(Func<TItem, IActivityForEachParallel<TItem>, CancellationToken, Task> method, CancellationToken cancellationToken = default);
    }
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.ForEachParallel"/>.
    /// </summary>
    public interface IActivityForEachParallel<TActivityReturns, out TItem> : IActivity
    {
        /// <summary>
        /// The items to loop over
        /// </summary>
        IEnumerable<TItem> Items { get; }

        /// <summary>
        /// Execute the <paramref name="method"/> for all items.
        /// </summary>
        /// <returns>A dictionary that associates each item with a result</returns>
        Task<IDictionary<string, TActivityReturns>> ExecuteAsync(
            Func<TItem, IActivityForEachParallel<TActivityReturns, TItem>, CancellationToken, Task<TActivityReturns>> method, 
            CancellationToken cancellationToken = default);
    }
}