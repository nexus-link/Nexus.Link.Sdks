using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

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
        [Obsolete($"Please use Action with {nameof(IActivityAction.SetMaxTime)}. Obsolete since 2023-09-12.", true)]
        public IActivityFlow SetMaxExecutionTimeSpan(TimeSpan timeSpan)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = timeSpan;
            return this;
        }

        /// <inheritdoc />
        [Obsolete($"Please use Action with {nameof(IActivityAction.SetMaxTime)}. Obsolete since 2022-05-09.", true)]
        public IActivityFlow SetDeadLine(DateTimeOffset deadline)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = deadline.Subtract(DateTimeOffset.UtcNow);
            return this;
        }

        /// <inheritdoc />
        [Obsolete($"Please use Action with {nameof(IActivityAction.SetMaxTime)}. Obsolete since 2023-09-12.", true)]
        public IActivityFlow SetDeadline(DateTimeOffset deadline)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = deadline.Subtract(DateTimeOffset.UtcNow);
            return this;
        }

        /// <inheritdoc/>
        [Obsolete("Please use Action(method). Obsolete since 2022-05-01")]
        public IActivityAction Action()
        {
            VerifyActualType(ActivityTypeEnum.Action);
            try
            {
                return new ActivityAction(ActivityInformation);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        private void VerifyActualType(ActivityTypeEnum actualType)
        {
            ActivityInformation.Type ??= actualType;
            InternalContract.Require(ActivityInformation.Type == actualType,
                $"The activity of type was declared as {ActivityInformation.Type}.");
        }

        /// <inheritdoc />
        public IActivityAction Action(ActivityMethodAsync<IActivityAction> methodAsync)
        {
            VerifyActualType(ActivityTypeEnum.Action);
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
            VerifyActualType(ActivityTypeEnum.Action);
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
            VerifyActualType(ActivityTypeEnum.Sleep);
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
            VerifyActualType(ActivityTypeEnum.Parallel);

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
            VerifyActualType(ActivityTypeEnum.If);
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
            VerifyActualType(ActivityTypeEnum.If);
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
            VerifyActualType(ActivityTypeEnum.If);
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
        public IActivityLock Lock(string resourceIdentifier = null)
        {
            VerifyActualType(ActivityTypeEnum.Lock);
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
        [Obsolete($"Please use {nameof(Action)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
        public IActivityThrottle Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan)
        {
            VerifyActualType(ActivityTypeEnum.Throttle);
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
            VerifyActualType(ActivityTypeEnum.Switch);
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
            VerifyActualType(ActivityTypeEnum.Switch);
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
            VerifyActualType(ActivityTypeEnum.Switch);
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
            VerifyActualType(ActivityTypeEnum.LoopUntilTrue);
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
            VerifyActualType(ActivityTypeEnum.LoopUntilTrue);
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
            VerifyActualType(ActivityTypeEnum.DoWhileOrUntil);
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
            VerifyActualType(ActivityTypeEnum.WhileDo);
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
            VerifyActualType(ActivityTypeEnum.WhileDo);
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
            VerifyActualType(ActivityTypeEnum.WhileDo);
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
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            VerifyActualType(ActivityTypeEnum.ForEachParallel);
            try
            {
                return new ActivityForEachParallel<TItem>(ActivityInformation, items, getIterationTitleMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, ActivityForEachParallelMethodAsync<TItem> methodAsync, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            VerifyActualType(ActivityTypeEnum.ForEachParallel);
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityForEachParallel<TItem>(ActivityInformation, items, methodAsync, getIterationTitleMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            VerifyActualType(ActivityTypeEnum.ForEachSequential);
            try
            {
                return new ActivityForEachSequential<TItem>(ActivityInformation, items, getIterationTitleMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TItem> methodAsync, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            VerifyActualType(ActivityTypeEnum.ForEachSequential);
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            try
            {
                return new ActivityForEachSequential<TItem>(ActivityInformation, items, getIterationTitleMethod, methodAsync);
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
            VerifyActualType(ActivityTypeEnum.Semaphore);
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
        [Obsolete($"Please use Action with {nameof(IActivityAction.SetMaxTime)}. Obsolete since 2023-09-12.", true)]
        public IActivityFlow<TActivityReturns> SetMaxExecutionTimeSpan(TimeSpan timeSpan)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = timeSpan;
            return this;
        }

        /// <inheritdoc />
        [Obsolete($"Please use Action with {nameof(IActivityAction.SetMaxTime)}. Obsolete since 2022-05-09.", true)]
        public IActivityFlow<TActivityReturns> SetDeadLine(DateTimeOffset deadline)
        {
            ActivityInformation.Options.ActivityMaxExecutionTimeSpan = deadline.Subtract(DateTimeOffset.UtcNow);
            return this;
        }

        /// <inheritdoc />
        [Obsolete($"Please use Action with {nameof(IActivityAction.SetMaxTime)}. Obsolete since 2023-09-12.", true)]
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
            VerifyActualType(ActivityTypeEnum.Action);
            try
            {
                return new ActivityAction<TActivityReturns>(ActivityInformation, DefaultValueForNotUrgentFail);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        private void VerifyActualType(ActivityTypeEnum actualType)
        {
            ActivityInformation.Type ??= actualType;
            InternalContract.Require(ActivityInformation.Type == actualType,
                $"The activity of type was declared as {ActivityInformation.Type}.");
        }

        /// <inheritdoc />
        public IActivityAction<TActivityReturns> Action(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync)
        {
            VerifyActualType(ActivityTypeEnum.Action);
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
            VerifyActualType(ActivityTypeEnum.Action);
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
            VerifyActualType(ActivityTypeEnum.Action);
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
            VerifyActualType(ActivityTypeEnum.LoopUntilTrue);
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
            VerifyActualType(ActivityTypeEnum.LoopUntilTrue);
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
            VerifyActualType(ActivityTypeEnum.DoWhileOrUntil);
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
            VerifyActualType(ActivityTypeEnum.WhileDo);
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
            VerifyActualType(ActivityTypeEnum.WhileDo);
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
            VerifyActualType(ActivityTypeEnum.WhileDo);
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
            VerifyActualType(ActivityTypeEnum.Condition);
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
            VerifyActualType(ActivityTypeEnum.If);
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
            VerifyActualType(ActivityTypeEnum.If);
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
            VerifyActualType(ActivityTypeEnum.If);
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
        public IActivityLock<TActivityReturns> Lock(string resourceIdentifier = null)
        {
            VerifyActualType(ActivityTypeEnum.Lock);
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
        [Obsolete($"Please use {nameof(Action<TActivityReturns>)} with {nameof(IActivityAction.WithThrottle)}. Obsolete since 2023-06-29.")]
        public IActivityThrottle<TActivityReturns> Throttle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan)
        {
            VerifyActualType(ActivityTypeEnum.Throttle);
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
            VerifyActualType(ActivityTypeEnum.Switch);
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
            VerifyActualType(ActivityTypeEnum.Switch);
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
            VerifyActualType(ActivityTypeEnum.Switch);
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
        public IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, GetKeyMethod<TItem> getKeyMethod, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            VerifyActualType(ActivityTypeEnum.ForEachParallel);
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachParallel)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachParallel<TActivityReturns, TItem>(ActivityInformation, items, getKeyMethod, getIterationTitleMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items, GetKeyMethod<TItem> getKeyMethod, ActivityForEachParallelMethodAsync<TActivityReturns, TItem> methodAsync, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            VerifyActualType(ActivityTypeEnum.ForEachParallel);
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachParallel)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachParallel<TActivityReturns, TItem>(ActivityInformation, items, getKeyMethod, methodAsync, getIterationTitleMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Please use ForEachSequential() with a method parameter. Obsolete since 2022-05-01")]
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            InternalContract.RequireNotNull(items, nameof(items));
            VerifyActualType(ActivityTypeEnum.ForEachSequential);
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachSequential)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachSequential<TActivityReturns, TItem>(ActivityInformation, items, getIterationTitleMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }

        /// <inheritdoc />
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items, ActivityForEachSequentialMethodAsync<TActivityReturns, TItem> methodAsync, GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        {
            VerifyActualType(ActivityTypeEnum.ForEachSequential);
            InternalContract.RequireNotNull(items, nameof(items));
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            InternalContract.Require(DefaultValueForNotUrgentFail == null, $"The activity {nameof(ForEachSequential)} does not support {nameof(SetDefaultValueForNotUrgentFail)}.");
            try
            {
                return new ActivityForEachSequential<TActivityReturns, TItem>(ActivityInformation, items, methodAsync, getIterationTitleMethod);
            }
            catch (Exception e)
            {
                throw CreateExceptionBecauseActivityConstructorFailed(ActivityInformation, e);
            }
        }
    }
}