using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityFlow
    {
        IActivityFlow SetParameter<T>(string name, T value);
        IActivityFlow SetParent(Activity parent);
        IActivityFlow SetPrevious(Activity previous);

        ActivityAction Action();
        [Obsolete("Use Action().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        Task ExecuteActionAsync(Func<ActivityAction, CancellationToken, Task> method, CancellationToken cancellationToken);
        [Obsolete("Use Action().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        Task<TMethodReturnType> ExecuteActionAsync<TMethodReturnType>(Func<ActivityAction, CancellationToken, Task<TMethodReturnType>> method, CancellationToken cancellationToken);
        
        ActivityCondition<bool> If();
        ActivityCondition<T> Condition<T>();
        [Obsolete("Use If().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        Task<bool> IfAsync(Func<Activity, CancellationToken, Task<bool>> ifMethodAsync, CancellationToken cancellationToken);

        [Obsolete("Use LoopUntil().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        ActivityLoopUntilTrue LoopUntil();
        Task<TMethodReturnType> LoopUntilTrueAsync<TMethodReturnType>(Func<ActivityLoopUntilTrue, CancellationToken, Task<TMethodReturnType>> methodAsync, CancellationToken cancellationToken);

        ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);

        [Obsolete("Use ForEachParallel().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        Task ForEachParallelAsync<TItem>(IEnumerable<TItem> items, Func<TItem, ActivityForEachParallel<TItem>, CancellationToken, Task> methodAsync, CancellationToken cancellationToken);
    }
}