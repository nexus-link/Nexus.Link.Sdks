﻿using System;
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

        /// <summary>
        /// Do an activity under a lock. This protects from other workflow instances of the same workflow form.
        /// If you want to protect from all other instances, no matter from which workflow form, then please use
        /// <see cref="Throttle"/>
        /// </summary>
        /// <param name="resourceIdentifier">The resource that should be locked, or null for a general lock.</param>
        /// <returns></returns>
        IActivityLock Lock(string resourceIdentifier);

        /// <summary>
        /// Do an activity that uses a resource that needs throttling. This makes sure that there is a limited of concurrent
        /// workflows that use this resource (for all workflow instances, no matter which workflow form they belong to).
        /// </summary>
        /// <param name="resourceIdentifier">The resource that needs throttling. Can not be null.</param>
        /// <param name="limit">The max number of instances that can use the resource at any given time.</param>
        /// <returns></returns>
        IActivityThrottle Throttle(string resourceIdentifier, int limit);

        IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TSwitchValue>, TSwitchValue> switchValueMethodAsync);
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



        /// <summary>
        /// Do an activity under a lock. This protects from other workflow instances of the same workflow form.
        /// If you want to protect from all other instances, no matter from which workflow form, then please use
        /// <see cref="Throttle"/>
        /// </summary>
        /// <param name="resourceIdentifier">The resource that should be locked, or null for a general lock.</param>
        /// <returns></returns>
        IActivityLock<TActivityReturns> Lock(string resourceIdentifier);

        /// <summary>
        /// Do an activity that uses a resource that needs throttling. This makes sure that there is a limited of concurrent
        /// workflows that use this resource (for all workflow instances, no matter which workflow form they belong to).
        /// </summary>
        /// <param name="resourceIdentifier">The resource that needs throttling. Can not be null.</param>
        /// <param name="limit">The max number of instances that can use the resource at any given time.</param>
        /// <returns></returns>
        IActivityThrottle<TActivityReturns> Throttle(string resourceIdentifier, int limit);

        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TActivityReturns, TSwitchValue>, TSwitchValue> switchValueMethodAsync);
        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod);
        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue);
        IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, Func<TItem, string> getKeyMethod);
        IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
    }
}