using System;
using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityFlowBase
    {
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

        /// <summary>
        /// When the time spent since the activity started passes this time, it will throw an <see cref="ActivityFailedException"/>.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        IActivityFlow SetMaxExecutionTime(TimeSpan timeSpan);

        IActivityAction Action();
        IActivitySleep Sleep(TimeSpan timeToSleep);
        IActivityParallel Parallel();
        IActivityIf If(ActivityIfConditionMethodAsync conditionMethodAsync);
        IActivityIf If(ActivityIfConditionMethod conditionMethod);
        IActivityIf If(bool condition);

        IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethodAsync<TSwitchValue> switchValueMethodAsync);
        IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod);
        IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue);

        IActivityLoopUntilTrue LoopUntil();
        IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceIdentifier">A string that uniquely identifies the resource that is protected by the semaphore.</param>
        /// <returns></returns>
        IActivitySemaphore Semaphore(string resourceIdentifier);
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

        /// <summary>
        /// When the time spent since the activity started passes this time, it will throw an <see cref="ActivityFailedException"/>.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        IActivityFlow<TActivityReturns> SetMaxExecutionTimeSpan(TimeSpan timeSpan);

        IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(TActivityReturns defaultValue);
        IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(Func<TActivityReturns> getDefaultValueMethod);
        IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync);

        IActivityAction<TActivityReturns> Action();
        IActivityLoopUntilTrue<TActivityReturns> LoopUntil();
        [Obsolete("Please use ActivityIf. Obsolete since 2022-04-27.")]
        IActivityCondition<TActivityReturns> Condition();
        IActivityIf<TActivityReturns> If(ActivityIfConditionMethodAsync conditionMethodAsync);
        IActivityIf<TActivityReturns> If(ActivityIfConditionMethod conditionMethod);
        IActivityIf<TActivityReturns> If(bool condition);
        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethodAsync<TSwitchValue> switchValueMethodAsync);
        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod);
        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue);
        IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, Func<TItem, string> getKeyMethod);
        IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
}