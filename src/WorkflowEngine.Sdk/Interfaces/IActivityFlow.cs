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

    /// <summary>
    /// Interface for flow programming when creating an activity
    /// </summary>
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

        [Obsolete("Please use Action() with a method parameter. Obsolete since 2022-05-01")]
        IActivityAction Action();
        IActivityAction Action(ActivityMethodAsync<IActivityAction> methodAsync);
        IActivityAction Action(ActivityMethod<IActivityAction> method);
        IActivitySleep Sleep(TimeSpan timeToSleep);
        IActivityParallel Parallel();
        IActivitySequential Sequential();

        IActivityIf If(ActivityConditionMethodAsync conditionMethodAsync);
        IActivityIf If(ActivityConditionMethod conditionMethod);
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
        /// <param name="limitationTimeSpan">The time span that the <paramref name="limit"/> applies to. Null means that the <paramref name="limit"/> is for concurrent instances.</param>
        /// <returns></returns>
        IActivityThrottle Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan = null);

        IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TSwitchValue>, TSwitchValue> switchValueMethodAsync);
        IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod);
        IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue);

        [Obsolete("Please use LoopUntil() with a method parameter. Obsolete since 2022-05-01")]
        IActivityLoopUntilTrue LoopUntil();

        [Obsolete("Please use Do or While. Obsolete since 2022-05-02.")]
        IActivityLoopUntilTrue LoopUntil(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync);

        IActivityDoWhileOrUntil Do(ActivityMethodAsync<IActivityDoWhileOrUntil> methodAsync);

        IActivityWhileDo While(ActivityConditionMethodAsync conditionMethodAsync);
        IActivityWhileDo While(ActivityConditionMethod conditionMethod);
        IActivityWhileDo While(bool condition);

        [Obsolete("Please use ForEachParallel() with a method parameter. Obsolete since 2022-05-01")]
        IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items);
        IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, ActivityForEachParallelMethodAsync<TItem> methodAsync);

        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
        IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TItem> methodAsync);
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


        [Obsolete("Please use Action() with a method parameter. Obsolete since 2022-05-01")]
        IActivityAction<TActivityReturns> Action();
        IActivityAction<TActivityReturns> Action(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync);
        IActivityAction<TActivityReturns> Action(ActivityMethod<IActivityAction<TActivityReturns>, TActivityReturns> method);
        IActivityAction<TActivityReturns> Action(TActivityReturns value);

        [Obsolete("Please use LoopUntil() with a method parameter. Obsolete since 2022-05-01")]
        IActivityLoopUntilTrue<TActivityReturns> LoopUntil();

        [Obsolete("Please use Do or While. Obsolete since 2022-05-02.")]
        IActivityLoopUntilTrue<TActivityReturns> LoopUntil(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync);

        IActivityDoWhileOrUntil<TActivityReturns> Do(ActivityMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>, TActivityReturns> methodAsync);

        IActivityWhileDo<TActivityReturns> While(ActivityConditionMethodAsync conditionMethodAsync);
        IActivityWhileDo<TActivityReturns> While(ActivityConditionMethod conditionMethod);
        IActivityWhileDo<TActivityReturns> While(bool condition);

        [Obsolete("Please use ActivityIf. Obsolete since 2022-04-27.")]
        IActivityCondition<TActivityReturns> Condition();

        IActivityIf<TActivityReturns> If(ActivityConditionMethodAsync conditionMethodAsync);
        IActivityIf<TActivityReturns> If(ActivityConditionMethod conditionMethod);
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
        /// <param name="limitationTimeSpan">The time span that the <paramref name="limit"/> applies to. Null means that the <paramref name="limit"/> is for concurrent instances.</param>
        /// <returns></returns>
        IActivityThrottle<TActivityReturns> Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan);

        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TActivityReturns, TSwitchValue>, TSwitchValue> switchValueMethodAsync);
        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod);
        IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue);

        [Obsolete("Please use ForEachParallel() with a method parameter. Obsolete since 2022-05-01")]
        IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, Func<TItem, string> getKeyMethod);
        IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, ActivityForEachParallelMethodAsync<TActivityReturns, TItem> methodAsync, Func<TItem, string> getKeyMethod);

        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items);
        IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TActivityReturns, TItem> methodAsync);
    }
}