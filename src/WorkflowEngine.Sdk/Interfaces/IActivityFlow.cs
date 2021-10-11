using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityFlow
    {
        Task ExecuteActionAsync(Func<ActivityAction, CancellationToken, Task> method, CancellationToken cancellationToken);
        Task<TMethodReturnType> ExecuteActionAsync<TMethodReturnType>(Func<ActivityAction, CancellationToken, Task<TMethodReturnType>> method, CancellationToken cancellationToken);
        Task ForEachParallelAsync<TItem>(IEnumerable<TItem> items, Func<TItem, ActivityForEachParallel<TItem>, CancellationToken, Task> methodAsync, CancellationToken cancellationToken);
        Task<bool> IfAsync(Func<Activity, CancellationToken, Task<bool>> ifMethodAsync, CancellationToken cancellationToken);
        Task<TMethodReturnType> LoopUntilTrueAsync<TMethodReturnType>(Func<ActivityLoopUntilTrue, CancellationToken, Task<TMethodReturnType>> methodAsync, CancellationToken cancellationToken);
        IActivityFlow SetParameter<T>(string name, T value);
        IActivityFlow SetParent(Activity parent);
        IActivityFlow SetPrevious(Activity previous);
    }
}