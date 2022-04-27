using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
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
        public IActivityAction Action()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityAction(ActivityInformation);
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
        public IActivityIf If(ActivityIfConditionMethodAsync conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            return new ActivityIf(ActivityInformation, conditionMethodAsync);
        }

        /// <inheritdoc />
        public IActivityIf If(ActivityIfConditionMethod conditionMethod)
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
        public IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethodAsync<TSwitchValue> switchValueMethodAsync)
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
        public IActivityLoopUntilTrue LoopUntil()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityLoopUntilTrue(ActivityInformation);
        }
        
        /// <inheritdoc/>
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachParallel<TItem>(ActivityInformation, items);
        }
        
        /// <inheritdoc/>
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachSequential<TItem>(ActivityInformation, items);
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
        public ActivityDefaultValueMethodAsync<TActivityReturns> GetDefaultValueForNotUrgentFail { get; private set; }

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
            GetDefaultValueForNotUrgentFail = getDefaultValueAsync;
            return this;
        }

        /// <inheritdoc/>
        public IActivityAction<TActivityReturns> Action()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityAction<TActivityReturns>(ActivityInformation, GetDefaultValueForNotUrgentFail);
        }
        
        /// <inheritdoc/>
        public IActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityLoopUntilTrue<TActivityReturns>(ActivityInformation, GetDefaultValueForNotUrgentFail);
        }

        /// <inheritdoc />
        [Obsolete("Please use If. Obsolete since 2022-04-27.")]
        public IActivityCondition<TActivityReturns> Condition()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Condition, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityCondition<TActivityReturns>(ActivityInformation, GetDefaultValueForNotUrgentFail);
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(ActivityIfConditionMethodAsync conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            return new ActivityIf<TActivityReturns>(ActivityInformation, GetDefaultValueForNotUrgentFail, conditionMethodAsync);
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(ActivityIfConditionMethod conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            return new ActivityIf<TActivityReturns>(ActivityInformation, GetDefaultValueForNotUrgentFail, (a, _) => Task.FromResult(conditionMethod(a)));
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityIf<TActivityReturns>(ActivityInformation, GetDefaultValueForNotUrgentFail, (a, _) => Task.FromResult(condition));
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethodAsync<TSwitchValue> switchValueMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethodAsync, nameof(switchValueMethodAsync));
            return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, GetDefaultValueForNotUrgentFail, switchValueMethodAsync);
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethod, nameof(switchValueMethod));
            return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, GetDefaultValueForNotUrgentFail, (a, _) => Task.FromResult(switchValueMethod(a)));
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, GetDefaultValueForNotUrgentFail, (a, _) => Task.FromResult(switchValue));
        }

        /// <inheritdoc/>
        public IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, Func<TItem, string> getKeyMethod)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachParallel<TActivityReturns, TItem>(ActivityInformation, items, getKeyMethod);
        }
        
        /// <inheritdoc/>
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            return new ActivityForEachSequential<TActivityReturns, TItem>(ActivityInformation, GetDefaultValueForNotUrgentFail, items);
        }
    }
}