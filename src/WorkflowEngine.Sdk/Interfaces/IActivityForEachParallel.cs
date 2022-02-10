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
        IEnumerable<TItem> Items { get; }

        Task ExecuteAsync(Func<TItem, IActivityForEachParallel<TItem>, CancellationToken, Task> method, CancellationToken cancellationToken = default);
    }

    public interface IActivityForEachParallel<TActivityReturns, out TItem, TKey> : IActivity<TActivityReturns>
    {
        IEnumerable<TItem> Items { get; }

        Task<IDictionary<TKey, TActivityReturns>> ExecuteAsync(Func<TItem, IActivityForEachParallel<TActivityReturns, TItem, TKey>, CancellationToken, Task<TActivityReturns>> method, CancellationToken cancellationToken = default);
        IActivityForEachParallel<TActivityReturns, TItem, TKey> SetGetKeyMethod(Func<TItem, TKey> method);
    }
}