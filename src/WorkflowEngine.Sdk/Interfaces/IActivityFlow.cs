using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityFlow
    {
        IActivityFlow SetParameter<T>(string name, T value);
        IActivityFlow SetParent(Activity parent);
        IActivityFlow SetPrevious(Activity previous);

        IActivityFlow OnException(ActivityFailUrgencyEnum failUrgency);

        ActivityAction Action();
        ActivityLoopUntilTrue LoopUntil();
        ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
    public interface IActivityFlow<TActivityReturns>
    {
        IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value);
        IActivityFlow<TActivityReturns> SetParent(Activity parent);
        IActivityFlow<TActivityReturns> SetPrevious(Activity previous);
        
        IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, TActivityReturns defaultValue);
        IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<TActivityReturns> getDefaultValueMethod);
        IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync);

        ActivityAction<TActivityReturns> Action();
        ActivityIf<TActivityReturns> If();
        ActivityLoopUntilTrue<TActivityReturns> LoopUntil();
        ActivityForEachParallel<TActivityReturns, TItem, TKey> ForEachParallel<TItem, TKey>(IEnumerable<TItem> items);
        ActivityForEachParallel<TActivityReturns, TItem, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
}