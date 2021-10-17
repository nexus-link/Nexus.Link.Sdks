using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
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
            var activityInformation = new ActivityInformation(WorkflowCapability, WorkflowInformation, MethodHandler,
                1, activityType, Previous?.ActivityInformation, Parent?.ActivityInformation)
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
            var activityInstance = new ActivityAction(activityInformation, AsyncRequestClient, Previous, Parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityCondition<bool> If()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityInstance = new ActivityCondition<bool>(activityInformation, AsyncRequestClient, Previous, Parent, null);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue LoopUntil()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityInstance = new ActivityLoopUntilTrue(activityInformation, AsyncRequestClient , Previous, Parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachParallel<TItem>(activityInformation, AsyncRequestClient, items, Previous, Parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachSequential<TItem>(activityInformation, AsyncRequestClient, items, Previous, Parent);
            return activityInstance;
        }
    }

    internal class ActivityFlow<TActivityReturns> : ActivityFlowBase, IActivityFlow<TActivityReturns>
    {
        public Func<Task<TActivityReturns>> GetDefaultValueMethodAsync { get; private set; }

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
            return OnException(failUrgency, () => Task.FromResult(defaultValue));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<TActivityReturns> getDefaultValueMethod)
        {
            return OnException(failUrgency, () => Task.FromResult(getDefaultValueMethod()));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<Task<TActivityReturns>> getDefaultValueMethodAsync)
        {
            FailUrgency = failUrgency;
            GetDefaultValueMethodAsync = getDefaultValueMethodAsync;
            return this;
        }

        /// <inheritdoc/>
        public ActivityAction<TActivityReturns> Action()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction<TActivityReturns>(activityInformation, AsyncRequestClient, Previous, Parent, GetDefaultValueMethodAsync);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityCondition<bool> If()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityInstance = new ActivityCondition<bool>(activityInformation, AsyncRequestClient, Previous, Parent, GetDefaultValueMethodAsync);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityInstance = new ActivityLoopUntilTrue<TActivityReturns>(activityInformation, AsyncRequestClient , Previous, Parent, GetDefaultValueMethodAsync);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachParallel<TActivityReturns, TItem>(activityInformation, AsyncRequestClient, items, Previous, Parent, GetDefaultValueMethodAsync);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachSequential<TActivityReturns, TItem>(activityInformation, AsyncRequestClient, items, Previous, Parent, GetDefaultValueMethodAsync);
            return activityInstance;
        }
    }
}