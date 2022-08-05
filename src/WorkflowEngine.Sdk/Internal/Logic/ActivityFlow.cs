using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal abstract class ActivityFlowBase : IActivityFlowBase
    {
        protected IActivityInformation ActivityInformation { get; }

        protected ActivityFlowBase(IActivityInformation activityInformation)
        {
            ActivityInformation = activityInformation;
        }

        protected Exception CreateExceptionBecauseActivityConstructorFailed(IActivityInformation activityInformation, Exception exception)
        {
            return new WorkflowImplementationShouldNotCatchThisException(new WorkflowEngineInternalErrorException(ActivityInformation, exception));
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
        public IActivityFlow SetExceptionAlertHandler(ActivityExceptionAlertMethodAsync alertMethodAsync)
        {
            ActivityInformation.Options.ExceptionAlertHandler = alertMethodAsync;
            return this;
        }

        /// <inheritdoc />
        [Obsolete("This will not be supported. Please use Action+Catch. Obsolete since 2022-06-15.")]
        public IActivityFlow SetExceptionHandler(ActivityExceptionHandlerAsync exceptionHandlerAsync)
        {
#pragma warning disable CS0618
            ActivityInformation.Options.ExceptionHandler = exceptionHandlerAsync;
#pragma warning restore CS0618
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
        public IActivityFlow SetMaxExecutionTimeSpan(TimeSpan timeSpan)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = timeSpan;
            return this;
        }

        /// <inheritdoc />
        [Obsolete($"Please use {nameof(SetDeadline)}. Obsolete since 2022-05-09.")]
        public IActivityFlow SetDeadLine(DateTimeOffset deadline)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = deadline.Subtract(DateTimeOffset.UtcNow);
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetDeadline(DateTimeOffset deadline)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = deadline.Subtract(DateTimeOffset.UtcNow);
            return this;
        }

        /// <inheritdoc/>
        [Obsolete("Please use Action(method). Obsolete since 2022-05-01")]
        public IActivityAction Action()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityAction(ActivityInformation);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityAction Action(ActivityMethodAsync<IActivityAction> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityAction(ActivityInformation, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityAction Action(ActivityMethod<IActivityAction> method)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(method, nameof(method));
            try
            {
                return new ActivityAction(ActivityInformation, (a, _) =>
          {
              method(a);
              return Task.CompletedTask;
          });
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivitySleep Sleep(TimeSpan timeToSleep)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Sleep, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivitySleep(ActivityInformation, timeToSleep);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityParallel Parallel()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Parallel, $"The activity was declared as {ActivityInformation.Type}.");

            try
            {
                return new ActivityParallel(ActivityInformation);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityIf If(ActivityConditionMethodAsync<IActivityIf> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            try
            {
                return new ActivityIf(ActivityInformation, conditionMethodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityIf If(ActivityConditionMethod<IActivityIf> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            try
            {
                return new ActivityIf(ActivityInformation, (a, _) => Task.FromResult(conditionMethod(a)));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityIf If(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityIf(ActivityInformation, (_, _) => Task.FromResult(condition));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityLock Lock(string resourceIdentifier)
        {
            if (resourceIdentifier != null)
            {
                InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier), $"The parameter {nameof(resourceIdentifier)} must not be empty and not only contain whitespace.");
            }
            try
            {
                var semaphoreSupport = new SemaphoreSupport(resourceIdentifier);
                return new ActivityLock(ActivityInformation, semaphoreSupport);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityThrottle Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan)
        {
            InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));

            try
            {
                var semaphoreSupport = new SemaphoreSupport(resourceIdentifier, limit, limitationTimeSpan);
                return new ActivityThrottle(ActivityInformation, semaphoreSupport);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TSwitchValue>, TSwitchValue> switchValueMethodAsync)
            where TSwitchValue : IComparable, IComparable<TSwitchValue>
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethodAsync, nameof(switchValueMethodAsync));
            try
            {
                return new ActivitySwitch<TSwitchValue>(ActivityInformation, switchValueMethodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod)
            where TSwitchValue : IComparable, IComparable<TSwitchValue>
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethod, nameof(switchValueMethod));
            try
            {
                return new ActivitySwitch<TSwitchValue>(ActivityInformation, (a, _) => Task.FromResult(switchValueMethod(a)));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivitySwitch<TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue)
            where TSwitchValue : IComparable, IComparable<TSwitchValue>
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivitySwitch<TSwitchValue>(ActivityInformation, (_, _) => Task.FromResult(switchValue));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use LoopUntil() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityLoopUntilTrue LoopUntil()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityLoopUntilTrue(ActivityInformation);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        [Obsolete("Please use Do or While. Obsolete since 2022-05-02.")]
        public IActivityLoopUntilTrue LoopUntil(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityLoopUntilTrue(ActivityInformation, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityDoWhileOrUntil Do(ActivityMethodAsync<IActivityDoWhileOrUntil> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.DoWhileOrUntil, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityDoWhileOrUntil(ActivityInformation, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityWhileDo While(ActivityConditionMethodAsync<IActivityWhileDo> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            try
            {
                return new ActivityWhileDo(ActivityInformation, conditionMethodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityWhileDo While(ActivityConditionMethod<IActivityWhileDo> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            try
            {
                return new ActivityWhileDo(ActivityInformation, (a, _) => Task.FromResult(conditionMethod(a)));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityWhileDo While(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityWhileDo(ActivityInformation, (_, _) => Task.FromResult(condition));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachParallel() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityForEachParallel<TItem>(ActivityInformation, items);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, ActivityForEachParallelMethodAsync<TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityForEachParallel<TItem>(ActivityInformation, items, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityForEachSequential<TItem>(ActivityInformation, items);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityForEachSequential<TItem>(ActivityInformation, items, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        [Obsolete($"Please use {nameof(Lock)} to lock within a workflow form and {nameof(Throttle)} to reduce the number of concurrent calls to a common resource (over all workflows).")]
        public IActivitySemaphore Semaphore(string resourceIdentifier)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Semaphore, $"The activity was declared as {ActivityInformation.Type}.");
#pragma warning restore CS0618 // Type or member is obsolete
            try
            {
                return new ActivitySemaphore(ActivityInformation, resourceIdentifier);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
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
        public IActivityFlow<TActivityReturns> SetExceptionAlertHandler(ActivityExceptionAlertMethodAsync alertMethodAsync)
        {
            ActivityInformation.Options.ExceptionAlertHandler = alertMethodAsync;
            return this;
        }

        /// <inheritdoc />
        [Obsolete("This will not be supported. Please use Action+Catch. Obsolete since 2022-06-15.")]
        public IActivityFlow<TActivityReturns> SetExceptionHandler(ActivityExceptionHandlerAsync exceptionHandlerAsync)
        {
            ActivityInformation.Options.ExceptionHandler = exceptionHandlerAsync;
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
        [Obsolete($"Please use {nameof(SetDeadline)}. Obsolete since 2022-05-09.")]
        public IActivityFlow<TActivityReturns> SetDeadLine(DateTimeOffset deadline)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = deadline.Subtract(DateTimeOffset.UtcNow);
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetDeadline(DateTimeOffset deadline)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = deadline.Subtract(DateTimeOffset.UtcNow);
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
            try
            {
                return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityAction<TActivityReturns> Action(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityAction<TActivityReturns> Action(ActivityMethod<IActivityAction<TActivityReturns>, TActivityReturns> method)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(method, nameof(method));
            try
            {
                return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(method(a)));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityAction<TActivityReturns> Action(TActivityReturns value)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Action, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (_, _) => Task.FromResult(value));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use LoopUntil() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityLoopUntilTrue<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        [Obsolete("Please use Do or While. Obsolete since 2022-05-02.")]
        public IActivityLoopUntilTrue<TActivityReturns> LoopUntil(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.LoopUntilTrue, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityLoopUntilTrue<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityDoWhileOrUntil<TActivityReturns> Do(ActivityMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>, TActivityReturns> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.DoWhileOrUntil, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityDoWhileOrUntil<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityWhileDo<TActivityReturns> While(ActivityConditionMethodAsync<IActivityWhileDo<TActivityReturns>> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            try
            {
                return new ActivityWhileDo<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, conditionMethodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityWhileDo<TActivityReturns> While(ActivityConditionMethod<IActivityWhileDo<TActivityReturns>> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            try
            {
                return new ActivityWhileDo<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(conditionMethod(a)));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityWhileDo<TActivityReturns> While(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.WhileDo, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityWhileDo<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (_, _) => Task.FromResult(condition));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        [Obsolete("Please use If. Obsolete since 2022-04-27.")]
        public IActivityCondition<TActivityReturns> Condition()
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Condition, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityCondition<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(ActivityConditionMethodAsync<IActivityIf<TActivityReturns>> conditionMethodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
            try
            {
                return new ActivityIf<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, conditionMethodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(ActivityConditionMethod<IActivityIf<TActivityReturns>> conditionMethod)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
            try
            {
                return new ActivityIf<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(conditionMethod(a)));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityIf<TActivityReturns> If(bool condition)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.If, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivityIf<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, (_, _) => Task.FromResult(condition));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityLock<TActivityReturns> Lock(string resourceIdentifier)
        {
            if (resourceIdentifier != null)
            {
                InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier), $"The parameter {nameof(resourceIdentifier)} must not be empty and not only contain whitespace.");
            }

            try
            {
                var semaphoreSupport = new SemaphoreSupport(resourceIdentifier);
                return new ActivityLock<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, semaphoreSupport);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityThrottle<TActivityReturns> Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan)
        {
            InternalContract.RequireNotNullOrWhiteSpace(resourceIdentifier, nameof(resourceIdentifier));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));

            try
            {
                var semaphoreSupport = new SemaphoreSupport(resourceIdentifier, limit, limitationTimeSpan);
                return new ActivityThrottle<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail, semaphoreSupport);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivityMethodAsync<IActivitySwitch<TActivityReturns, TSwitchValue>, TSwitchValue> switchValueMethodAsync)
            where TSwitchValue : IComparable, IComparable<TSwitchValue>
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethodAsync, nameof(switchValueMethodAsync));
            try
            {
                return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, DefaultValueForNotUrgentFail, switchValueMethodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(ActivitySwitchValueMethod<TSwitchValue> switchValueMethod)
            where TSwitchValue : IComparable, IComparable<TSwitchValue>
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(switchValueMethod, nameof(switchValueMethod));
            try
            {
                return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, DefaultValueForNotUrgentFail, (a, _) => Task.FromResult(switchValueMethod(a)));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivitySwitch<TActivityReturns, TSwitchValue> Switch<TSwitchValue>(TSwitchValue switchValue)
            where TSwitchValue : IComparable, IComparable<TSwitchValue>
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.Switch, $"The activity was declared as {ActivityInformation.Type}.");
            try
            {
                return new ActivitySwitch<TActivityReturns, TSwitchValue>(ActivityInformation, DefaultValueForNotUrgentFail, (_, _) => Task.FromResult(switchValue));
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachParallel() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, GetKeyMethod<TItem> getKeyMethod)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachParallel)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachParallel<TActivityReturns, TItem>(ActivityInformation, items, getKeyMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, GetKeyMethod<TItem> getKeyMethod, ActivityForEachParallelMethodAsync<TActivityReturns, TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachParallel, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachParallel)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachParallel<TActivityReturns, TItem>(ActivityInformation, items, getKeyMethod, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachSequential)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachSequential<TActivityReturns, TItem>(ActivityInformation, items);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TActivityReturns, TItem> methodAsync)
        {
            InternalContract.Require(ActivityInformation.Type == ActivityTypeEnum.ForEachSequential, $"The activity was declared as {ActivityInformation.Type}.");
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachSequential)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachSequential<TActivityReturns, TItem>(ActivityInformation, items, methodAsync);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }
    }
}