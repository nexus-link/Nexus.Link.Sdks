using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic.Activities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IInternalActivityFlow
    {
        IWorkflowVersion WorkflowVersion { get; }
        WorkflowPersistence WorkflowPersistence { get; }
        MethodHandler MethodHandler { get; }
        string FormTitle { get; }
        string ActivityFormId { get; }
        ActivityFailUrgencyEnum FailUrgency { get; }
        int Position { get; }
    }

    public interface IActivityFlow
    {
        IActivityFlow SetParameter<T>(string name, T value);
        IActivityFlow OnException(ActivityFailUrgencyEnum failUrgency);

        ActivityAction Action();
        ActivityLoopUntilTrue LoopUntil();
        ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }

    public interface IActivityFlow<TActivityReturns>
    {
        IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value);
        
        IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, TActivityReturns defaultValue);
        IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<TActivityReturns> getDefaultValueMethod);
        IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync);

        ActivityAction<TActivityReturns> Action();
        ActivityLoopUntilTrue<TActivityReturns> LoopUntil();
        ActivityCondition<TActivityReturns> Condition();
        ActivityForEachParallel<TActivityReturns, TItem, TKey> ForEachParallel<TItem, TKey>(IEnumerable<TItem> items);
        ActivityForEachParallel<TActivityReturns, TItem, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
}