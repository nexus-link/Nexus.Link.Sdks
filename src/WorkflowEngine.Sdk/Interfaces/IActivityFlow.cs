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
        ActivityCondition<bool> If();
        ActivityLoopUntilTrue LoopUntil();
        ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
    public interface IActivityFlow<TActivityReturns>
    {
        IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value);
        IActivityFlow<TActivityReturns> SetParent(Activity parent);
        IActivityFlow<TActivityReturns> SetPrevious(Activity previous);

        ActivityAction<TActivityReturns> Action();
        ActivityCondition<bool> If();
        ActivityLoopUntilTrue<TActivityReturns> LoopUntil();
        ActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
}