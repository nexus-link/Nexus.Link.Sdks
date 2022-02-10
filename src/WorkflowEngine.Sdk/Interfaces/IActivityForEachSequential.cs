using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// An activity of type <see cref="ActivityTypeEnum.ForEachSequential"/>.
    /// </summary>
    public interface IActivityForEachSequential<out TItem> : IActivity
    {
        IEnumerable<TItem> Items { get; }

        Task ExecuteAsync(System.Func<TItem, IActivityForEachSequential<TItem>, CancellationToken, Task> method, CancellationToken cancellationToken = default);
    }

    public interface IActivityForEachSequential<TActivityReturns, out TITem> : IActivity<TActivityReturns>
    {
        IEnumerable<TITem> Items { get; }

        Task<IList<TActivityReturns>> ExecuteAsync(Func<TITem, IActivityForEachSequential<TActivityReturns, TITem>, CancellationToken, Task<TActivityReturns>> method, CancellationToken cancellationToken = default);
    }
}