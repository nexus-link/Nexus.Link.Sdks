using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    internal interface IInternalActivityFlow
    {
        WorkflowCache WorkflowCache { get; }
        WorkflowInformation WorkflowInformation { get; }
        MethodHandler MethodHandler { get; }
        int Position { get; }
        string FormTitle { get; }
        string ActivityFormId { get; }

        ActivityOptions Options { get; }
    }

    public interface IActivityFlowBase
    {
        string ActivityFormId { get; }
    }

    public interface IActivityFlow : IActivityFlowBase
    {
        IActivityFlow SetParameter<T>(string name, T value);
        IActivityFlow SetAsyncRequestPriority(double priority);
        IActivityFlow SetFailUrgency(ActivityFailUrgencyEnum failUrgency);
        IActivityFlow SetExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler);
        IActivityFlow SetLogCreateThreshold(LogSeverityLevel severityLevel);
        IActivityFlow SetPurgeLogStrategy(LogPurgeStrategyEnum logPurgeStrategy);
        IActivityFlow SetLogPurgeThreshold(LogSeverityLevel severityLevel);

        IActivityAction Action();
        IActivityLoopUntilTrue LoopUntil();
        IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }

    public interface 
        IActivityFlow<TActivityReturns> : IActivityFlowBase
    {
        IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value);
        IActivityFlow<TActivityReturns> SetAsyncRequestPriority(double priority);
        IActivityFlow<TActivityReturns> SetFailUrgency(ActivityFailUrgencyEnum failUrgency);
        IActivityFlow<TActivityReturns> SetExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler);
        IActivityFlow<TActivityReturns> SetLogCreateThreshold(LogSeverityLevel severityLevel);
        IActivityFlow<TActivityReturns> SetPurgeLogStrategy(LogPurgeStrategyEnum logPurgeStrategy);
        IActivityFlow<TActivityReturns> SetLogPurgeThreshold(LogSeverityLevel severityLevel);
        
        IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(TActivityReturns defaultValue);
        IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(Func<TActivityReturns> getDefaultValueMethod);
        IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync);

        IActivityAction<TActivityReturns> Action();
        IActivityLoopUntilTrue<TActivityReturns> LoopUntil();
        IActivityCondition<TActivityReturns> Condition();
        IActivityForEachParallel<TActivityReturns, TItem, TKey> ForEachParallel<TItem, TKey>(IEnumerable<TItem> items);
        IActivityForEachParallel<TActivityReturns, TItem, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
}