using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    internal abstract class ActivityFlowBase
    {
        protected readonly WorkflowInformation WorkflowInformation;
        protected readonly IWorkflowCapability WorkflowCapability;
        protected readonly IAsyncRequestClient AsyncRequestClient;
        protected readonly string ActivityFormId;
        protected readonly MethodHandler MethodHandler;
        protected readonly string FormTitle;
        protected Activity Parent;
        protected Activity Previous;
        protected ActivityFailUrgencyEnum FailUrgency;

        protected ActivityFlowBase(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowInformation workflowInformation, string formTitle, string activityFormId)
        {
            WorkflowInformation = workflowInformation;
            WorkflowCapability = workflowCapability;
            AsyncRequestClient = asyncRequestClient;
            ActivityFormId = activityFormId;
            FormTitle = formTitle;
            MethodHandler = new MethodHandler(formTitle);
        }

        protected ActivityInformation CreateActivityInformation(WorkflowActivityTypeEnum activityType)
        {
            var activityInformation = new ActivityInformation(WorkflowInformation, MethodHandler, 1,
                activityType, Previous?.ActivityInformation, Parent?.ActivityInformation)
            {
                FormId = ActivityFormId,
                FormTitle = FormTitle
            };
            return activityInformation;
        }
    }

    internal class ActivityFlow : ActivityFlowBase, IActivityFlow
    {

        public ActivityFlow(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowInformation workflowInformation, string formTitle, string activityFormId) 
        :base(workflowCapability, asyncRequestClient,workflowInformation, formTitle, activityFormId)
        {
        }

        public IActivityFlow SetParameter<T>(string name, T value)
        {
            MethodHandler.DefineParameter<T>(name);
            MethodHandler.SetParameter(name, value);
            return this;
        }

        public IActivityFlow SetParent(Activity parent)
        {
            Parent = parent;
            return this;
        }

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
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityAction(activityInformation, activityExecutor, Parent);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue LoopUntil()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityLoopUntilTrue(activityInformation, activityExecutor, Parent);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityForEachParallel<TItem>(activityInformation, activityExecutor, items, Parent);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityForEachSequential<TItem>(activityInformation, activityExecutor, items, Previous, Parent);
            return activity;
        }
    }

    internal class ActivityFlow<TActivityReturns> : ActivityFlowBase, IActivityFlow<TActivityReturns>
    {
        public Func<CancellationToken, Task<TActivityReturns>> GetDefaultValueMethodAsync { get; private set; }

        public ActivityFlow(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowInformation workflowInformation, string formTitle, string activityFormId) 
        :base(workflowCapability, asyncRequestClient,workflowInformation, formTitle, activityFormId)
        {
        }

        public IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value)
        {
            MethodHandler.DefineParameter<T>(name);
            MethodHandler.SetParameter(name, value);
            return this;
        }

        public IActivityFlow<TActivityReturns> SetParent(Activity parent)
        {
            Parent = parent;
            return this;
        }

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
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityAction<TActivityReturns>(activityInformation, activityExecutor, Parent, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityIf<TActivityReturns> If()
        {
            InternalContract.Require(typeof(TActivityReturns) == typeof(bool), $"You can only use {nameof(If)}() with type {nameof(Boolean)}, not with type {nameof(TActivityReturns)}." );
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityIf<TActivityReturns>(activityInformation, activityExecutor, Parent, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityLoopUntilTrue<TActivityReturns>(activityInformation, activityExecutor, Parent, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityForEachParallel<TActivityReturns, TItem>(activityInformation, activityExecutor, items, Parent, GetDefaultValueMethodAsync);
            return activity;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityExecutor = new ActivityExecutor(AsyncRequestClient);
            var activity = new ActivityForEachSequential<TActivityReturns, TItem>(activityInformation, activityExecutor, items, Parent, GetDefaultValueMethodAsync);
            return activity;
        }
    }
}