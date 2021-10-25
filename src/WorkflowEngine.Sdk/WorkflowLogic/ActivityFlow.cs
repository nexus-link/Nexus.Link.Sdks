using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    internal abstract class ActivityFlowBase
    {
        protected IWorkflowVersionBase WorkflowVersion { get; }
        protected readonly WorkflowPersistence WorkflowPersistence;
        protected readonly IWorkflowCapability WorkflowCapability;
        protected readonly IAsyncRequestClient AsyncRequestClient;
        protected readonly string ActivityFormId;
        protected readonly MethodHandler MethodHandler;
        protected readonly string FormTitle;
        [Obsolete("No need anymore")]
        protected Activity Parent;
        [Obsolete("No need anymore")]
        protected Activity Previous;
        protected ActivityFailUrgencyEnum FailUrgency;

        protected ActivityFlowBase(IWorkflowVersionBase workflowVersion, IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId)
        {
            WorkflowVersion = workflowVersion;
            WorkflowPersistence = workflowPersistence;
            WorkflowCapability = workflowCapability;
            AsyncRequestClient = asyncRequestClient;
            ActivityFormId = activityFormId;
            FormTitle = formTitle;
            MethodHandler = new MethodHandler(formTitle);
        }

        protected ActivityPersistence CreateActivityInformation(ActivityTypeEnum activityType)
        {
            var activityInformation = new ActivityPersistence(WorkflowPersistence, MethodHandler,
                FormTitle, 1, ActivityFormId, activityType);
            return activityInformation;
        }
    }

    internal class ActivityFlow : ActivityFlowBase, IActivityFlow
    {

        public ActivityFlow(WorkflowVersionBase workflowVersion, IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId) 
        :base(workflowVersion, workflowCapability, asyncRequestClient,workflowPersistence, formTitle, activityFormId)
        {
        }

        public IActivityFlow SetParameter<T>(string name, T value)
        {
            MethodHandler.DefineParameter<T>(name);
            MethodHandler.SetParameter(name, value);
            return this;
        }

        [Obsolete("No need anymore")]
        public IActivityFlow SetParent(Activity parent)
        {
            Parent = parent;
            return this;
        }

        [Obsolete("No need anymore")]
        public IActivityFlow SetPrevious(Activity previous)
        {
            Previous = previous;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow OnException(ActivityFailUrgencyEnum failUrgency)
        {
            FailUrgency = failUrgency;
            return this;
        }

        /// <inheritdoc/>
        public ActivityAction Action()
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.Action);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityAction( activityInformation, activityExecutor);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue LoopUntil()
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.LoopUntilTrue);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityLoopUntilTrue(activityInformation, activityExecutor);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityForEachParallel<TItem>(activityInformation, activityExecutor, items);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityForEachSequential<TItem>(activityInformation, activityExecutor, items);
            return activity;
        }
    }

    internal class ActivityFlow<TActivityReturns> : ActivityFlowBase, IActivityFlow<TActivityReturns>
    {
        public Func<CancellationToken, Task<TActivityReturns>> GetDefaultValueMethodAsync { get; private set; }

        public ActivityFlow(WorkflowVersionBase workflowVersion, IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId) 
        :base(workflowVersion, workflowCapability, asyncRequestClient,workflowPersistence, formTitle, activityFormId)
        {
        }

        public IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value)
        {
            MethodHandler.DefineParameter<T>(name);
            MethodHandler.SetParameter(name, value);
            return this;
        }
        
        [Obsolete("No need anymore")]
        public IActivityFlow<TActivityReturns> SetParent(Activity parent)
        {
            Parent = parent;
            return this;
        }
        
        [Obsolete("No need anymore")]
        public IActivityFlow<TActivityReturns> SetPrevious(Activity previous)
        {
            Previous = previous;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, TActivityReturns defaultValue)
        {
            return OnException(failUrgency, ct => Task.FromResult(defaultValue));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<TActivityReturns> getDefaultValueMethod)
        {
            return OnException(failUrgency, ct => Task.FromResult(getDefaultValueMethod()));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
        {
            FailUrgency = failUrgency;
            GetDefaultValueMethodAsync = getDefaultValueMethodAsync;
            return this;
        }

        /// <inheritdoc/>
        public ActivityAction<TActivityReturns> Action()
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.Action);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityAction<TActivityReturns>(activityInformation, activityExecutor, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityIf<TActivityReturns> If()
        {
            InternalContract.Require(typeof(TActivityReturns) == typeof(bool), $"You can only use {nameof(If)}() with type {nameof(Boolean)}, not with type {nameof(TActivityReturns)}." );
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.Condition);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityIf<TActivityReturns>(activityInformation, activityExecutor, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.LoopUntilTrue);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityLoopUntilTrue<TActivityReturns>(activityInformation, activityExecutor, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TActivityReturns, TItem, TKey> ForEachParallel<TItem, TKey>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityForEachParallel<TActivityReturns, TItem, TKey>(activityInformation, activityExecutor, items, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TActivityReturns, TItem, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityForEachParallel<TActivityReturns, TItem, TItem>(activityInformation, activityExecutor, items, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(ActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(WorkflowVersion, AsyncRequestClient);
            var activity = new ActivityForEachSequential<TActivityReturns, TItem>(activityInformation, activityExecutor, items, GetDefaultValueMethodAsync);
            return activity;
        }
    }
}