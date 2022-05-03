using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal abstract class ActivityFlowBase : IActivityFlowBase
    {
        protected IActivityInformation ActivityInformation{ get; }

        protected ActivityFlowBase(IActivityInformation activityInformation)
        {
            ActivityInformation = activityInformation;
        }
    }

    internal class ActivityFlow : ActivityFlowBase, IActivityFlow
    {
        public ActivityFlow(IActivityInformation activityInformation)
        : base(activityInformation)
        {
        }

        public IActivityFlow SetParameter<T>(string name, T value)
        {
            ActivityInformation.DefineParameter<T>(name);
            ActivityInformation.SetParameter(name, value);
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetAsyncRequestPriority(double priority)
        {
            ActivityInformation.Options.AsyncRequestPriority = priority;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetFailUrgency(ActivityFailUrgencyEnum failUrgency)
        {
            ActivityInformation.Options.FailUrgency = failUrgency;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler)
        {
            ActivityInformation.Options.ExceptionAlertHandler = alertHandler;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetLogCreateThreshold(LogSeverityLevel severityLevel)
        {
            ActivityInformation.Options.LogCreateThreshold = severityLevel;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetPurgeLogStrategy(LogPurgeStrategyEnum logPurgeStrategy)
        {
            ActivityInformation.Options.LogPurgeStrategy = logPurgeStrategy;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetLogPurgeThreshold(LogSeverityLevel severityLevel)
        {
            ActivityInformation.Options.LogPurgeThreshold = severityLevel;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetMaxExecutionTime(TimeSpan timeSpan)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = timeSpan;
            return this;
        }

        /// <inheritdoc/>
        [Obsolete("Please use Action(method). Obsolete since 2022-05-01")]
        public IActivityAction Action()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityAction(ActivityInformation);
        }

        /// <inheritdoc />
        public IActivityAction Action(ActivityMethodAsync<IActivityAction> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityAction(ActivityInformation, methodAsync);
        }

        /// <inheritdoc />
        public IActivityAction Action(ActivityMethod<IActivityAction> method)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(method, nameof(method));
            return new ActivityAction(ActivityInformation, (a, _) =>
            {
                method(a);
                return Task.CompletedTask;
            });
        }

        /// <inheritdoc />
        public IActivitySleep Sleep(TimeSpan timeToSleep)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Sleep, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivitySleep(ActivityInformation, timeToSleep);
        }

        /// <inheritdoc />
        public IActivityParallel Parallel()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Parallel, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityParallel(ActivityInformation);
        }

        /// <inheritdoc />
        public IActivityIf If(ActivityConditionMethodAsync<IActivityIf> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            return new ActivityIf(ActivityInformation, conditionMethodAsync);
        }

        /// <inheritdoc />
        public IActivityIf If(ActivityConditionMethod<IActivityIf> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            return new ActivityIf(ActivityInformation, (a, _) => Task.FromResult(conditionMethod(a)));
        }

        /// <inheritdoc />
        public IActivityIf If(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityIf(ActivityInformation, (a, _) => Task.FromResult(condition));
        }

        /// <inheritdoc />
        public IActivityLock Lock(string resourceIdentifier)
        {
            if (resourceIdentifier != null)
            {
                InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier), $"The parameter {nameof(resourceIdentifier)} must not be empty and not only contain whitespace.");
            }
            var semaphoreSupport = new SemaphoreSupport(resourceIdentifier);
            return new ActivityLock(ActivityInformation, semaphoreSupport);
        }

        /// <inheritdoc />
        public IActivityThrottle Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan)
        {
            InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));

            var semaphoreSupport = new SemaphoreSupport(resourceIdentifier, limit, limitationTimeSpan);
            return new ActivityThrottle(ActivityInformation, semaphoreSupport);
        }

        /// <inheritdoc />
        public IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TSwitchValue>, TSwitchValue> switchValueMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethodAsync, nameof(switchValueMethodAsync));
            return new ActivitySwitch<TSwitchValue>(ActivityInformation, switchValueMethodAsync);
        }

        /// <inheritdoc />
        public IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethod, nameof(switchValueMethod));
            return new ActivitySwitch<TSwitchValue>(ActivityInformation, (a, _) => Task.FromResult(switchValueMethod(a)));
        }

        /// <inheritdoc />
        public IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivitySwitch<TSwitchValue>(ActivityInformation, (a, _) => Task.FromResult(switchValue));
        }

        /// <inheritdoc/>
        [Obsolete("Please use LoopUntil() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityLoopUntilTrue LoopUntil()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityLoopUntilTrue(ActivityInformation);
        }

        /// <inheritdoc />
        [Obsolete("Please use Do or While. Obsolete since 2022-05-02.")]
        public IActivityLoopUntilTrue LoopUntil(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityLoopUntilTrue(ActivityInformation, methodAsync);
        }

        /// <inheritdoc />
        public IActivityDoWhileOrUntil Do(ActivityMethodAsync<IActivityDoWhileOrUntil> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.DoWhileOrUntil, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityDoWhileOrUntil(ActivityInformation, methodAsync);
        }

        /// <inheritdoc />
        public IActivityWhileDo While(ActivityConditionMethodAsync<IActivityWhileDo> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            return new ActivityWhileDo(ActivityInformation, conditionMethodAsync);
        }

        /// <inheritdoc />
        public IActivityWhileDo While(ActivityConditionMethod<IActivityWhileDo> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            return new ActivityWhileDo(ActivityInformation, (a, _) => Task.FromResult(conditionMethod(a)));
        }

        /// <inheritdoc />
        public IActivityWhileDo While(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityWhileDo(ActivityInformation, (a, _) => Task.FromResult(condition));
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachParallel() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachParallel<TItem>(ActivityInformation, items);
        }

        /// <inheritdoc />
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, ActivityForEachParallelMethodAsync<TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityForEachParallel<TItem>(ActivityInformation, items, methodAsync);
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachSequential<TItem>(ActivityInformation, items);
        }

        /// <inheritdoc />
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityForEachSequential<TItem>(ActivityInformation, items, methodAsync);
        }

        /// <inheritdoc />
        public IActivitySemaphore Semaphore(string resourceIdentifier)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Semaphore, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivitySemaphore(ActivityInformation, resourceIdentifier);
        }
    }

    internal class ActivityFlow<TActivityReturns> : ActivityFlowBase, IActivityFlow<TActivityReturns>
    {
        public ActivityDefaultValueMethodAsync<TActivityReturns> DefaultValueForNotUrgentFail { get; private set; }

        public ActivityFlow(IActivityInformation activityInformation)
        : base(activityInformation)
        {
        }

        public IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value)
        {
            ActivityInformation.DefineParameter<T>(name);
            ActivityInformation.SetParameter(name, value);
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetAsyncRequestPriority(double priority)
        {
            ActivityInformation.Options.AsyncRequestPriority = priority;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetFailUrgency(ActivityFailUrgencyEnum failUrgency)
        {
            ActivityInformation.Options.FailUrgency = failUrgency;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler)
        {
            ActivityInformation.Options.ExceptionAlertHandler = alertHandler;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetLogCreateThreshold(LogSeverityLevel severityLevel)
        {
            ActivityInformation.Options.LogCreateThreshold = severityLevel;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetPurgeLogStrategy(LogPurgeStrategyEnum logPurgeStrategy)
        {
            ActivityInformation.Options.LogPurgeStrategy = logPurgeStrategy;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetLogPurgeThreshold(LogSeverityLevel severityLevel)
        {
            ActivityInformation.Options.LogPurgeThreshold = severityLevel;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetMaxExecutionTimeSpan(TimeSpan timeSpan)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = timeSpan;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(TActivityReturns defaultValue)
        {
            return SetDefaultValueForNotUrgentFail(_ => Task.FromResult(defaultValue));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(Func<TActivityReturns> getDefaultValueMethod)
        {
            return SetDefaultValueForNotUrgentFail(_ => Task.FromResult(getDefaultValueMethod()));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync)
        {
            DefaultValueForNotUrgentFail = getDefaultValueAsync;
            return this;
        }

        /// <inheritdoc/>
        [Obsolete("Please use Action() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityAction<TActivityReturns> Action()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail);
        }

        /// <inheritdoc />
        public IActivityAction<TActivityReturns> Action(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, methodAsync);
        }

        /// <inheritdoc />
        public IActivityAction<TActivityReturns> Action(ActivityMethod<IActivityAction<TActivityReturns>, TActivityReturns> method)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(method, nameof(method));
            return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(method(a)));
        }

        /// <inheritdoc />
        public IActivityAction<TActivityReturns> Action(TActivityReturns value)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(value));
        }

        /// <inheritdoc/>
        [Obsolete("Please use LoopUntil() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityLoopUntilTrue<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail);
        }

        /// <inheritdoc />
        [Obsolete("Please use Do or While. Obsolete since 2022-05-02.")]
        public IActivityLoopUntilTrue<TActivityReturns> LoopUntil(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityLoopUntilTrue<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, methodAsync);
        }

        /// <inheritdoc />
        public IActivityDoWhileOrUntil<TActivityReturns> Do(ActivityMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>, TActivityReturns> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.DoWhileOrUntil, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityDoWhileOrUntil<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, methodAsync);
        }

        /// <inheritdoc />
        public IActivityWhileDo<TActivityReturns> While(ActivityConditionMethodAsync<IActivityWhileDo<TActivityReturns>> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            return new ActivityWhileDo<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, conditionMethodAsync);
        }

        /// <inheritdoc />
        public IActivityWhileDo<TActivityReturns> While(ActivityConditionMethod<IActivityWhileDo<TActivityReturns>> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            return new ActivityWhileDo<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(conditionMethod(a)));
        }

        /// <inheritdoc />
        public IActivityWhileDo<TActivityReturns> While(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityWhileDo<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(condition));
        }

        /// <inheritdoc />
        [Obsolete("Please use If. Obsolete since 2022-04-27.")]
        public IActivityCondition<TActivityReturns> Condition()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Condition, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityCondition<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail);
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(ActivityConditionMethodAsync<IActivityIf<TActivityReturns>> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            return new ActivityIf<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, conditionMethodAsync);
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(ActivityConditionMethod<IActivityIf<TActivityReturns>> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            return new ActivityIf<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(conditionMethod(a)));
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityIf<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(condition));
        }

        /// <inheritdoc />
        public IActivityLock<TActivityReturns> Lock(string resourceIdentifier)
        {
            if (resourceIdentifier != null)
            {
                InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier), $"The parameter {nameof(resourceIdentifier)} must not be empty and not only contain whitespace.");
            }

            var semaphoreSupport = new SemaphoreSupport(resourceIdentifier);
            return new ActivityLock<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, semaphoreSupport);
        }

        /// <inheritdoc />
        public IActivityThrottle<TActivityReturns> Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan)
        {
            InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));

            var semaphoreSupport = new SemaphoreSupport(resourceIdentifier, limit, limitationTimeSpan);
            return new ActivityThrottle<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, semaphoreSupport);
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TActivityReturns, TSwitchValue>, TSwitchValue> switchValueMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethodAsync, nameof(switchValueMethodAsync));
            return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, DefaultValueForNotUrgentFail, switchValueMethodAsync);
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethod, nameof(switchValueMethod));
            return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(switchValueMethod(a)));
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(switchValue));
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachParallel() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, GetKeyMethod<TItem> getKeyMethod)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachParallel<TActivityReturns, TItem>(ActivityInformation, items, getKeyMethod);
        }

        /// <inheritdoc />
        public IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, GetKeyMethod<TItem> getKeyMethod, ActivityForEachParallelMethodAsync<TActivityReturns, TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityForEachParallel<TActivityReturns, TItem>(ActivityInformation, items, getKeyMethod, methodAsync);
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachSequential<TActivityReturns, TItem>(ActivityInformation, DefaultValueForNotUrgentFail, items);
        }

        /// <inheritdoc />
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TActivityReturns, TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return new ActivityForEachSequential<TActivityReturns, TItem>(ActivityInformation, DefaultValueForNotUrgentFail, items, methodAsync);
        }
    }
}