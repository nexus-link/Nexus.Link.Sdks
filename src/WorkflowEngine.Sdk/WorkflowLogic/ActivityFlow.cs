using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    internal abstract class ActivityFlowBase
    {
        protected readonly WorkflowInformation _workflowInformation;
        protected readonly IWorkflowCapability _workflowCapability;
        protected readonly IAsyncRequestClient _asyncRequestClient;
        protected readonly string _activityFormId;
        protected readonly MethodHandler _methodHandler;
        protected readonly string _formTitle;
        protected Activity _parent;
        protected Activity _previous;

        protected ActivityFlowBase(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowInformation workflowInformation, string formTitle, string activityFormId)
        {
            _workflowInformation = workflowInformation;
            _workflowCapability = workflowCapability;
            _asyncRequestClient = asyncRequestClient;
            _activityFormId = activityFormId;
            _formTitle = formTitle;
            _methodHandler = new MethodHandler(formTitle);
        }

        protected ActivityInformation CreateActivityInformation(WorkflowActivityTypeEnum activityType)
        {
            var activityInformation = new ActivityInformation(_workflowCapability, _workflowInformation, _methodHandler,
                1, activityType, _previous?.ActivityInformation, _parent?.ActivityInformation)
            {
                FormId = _activityFormId,
                FormTitle = _formTitle
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
            _methodHandler.DefineParameter<T>(name);
            _methodHandler.SetParameter(name, value);
            return this;
        }

        public IActivityFlow SetParent(Activity parent)
        {
            _parent = parent;
            return this;
        }

        public IActivityFlow SetPrevious(Activity previous)
        {
            _previous = previous;
            return this;
        }
        
        /// <inheritdoc/>
        public ActivityAction Action()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityCondition<bool> If()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityInstance = new ActivityCondition<bool>(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue LoopUntil()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityInstance = new ActivityLoopUntilTrue(activityInformation, _asyncRequestClient , _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachParallel<TItem>(activityInformation, _asyncRequestClient, items, _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachSequential<TItem>(activityInformation, _asyncRequestClient, items, _previous, _parent);
            return activityInstance;
        }
    }

    internal class ActivityFlow<TActivityReturns> : ActivityFlowBase, IActivityFlow<TActivityReturns>
    {

        public ActivityFlow(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowInformation workflowInformation, string formTitle, string activityFormId) 
        :base(workflowCapability, asyncRequestClient,workflowInformation, formTitle, activityFormId)
        {
        }

        public IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value)
        {
            _methodHandler.DefineParameter<T>(name);
            _methodHandler.SetParameter(name, value);
            return this;
        }

        public IActivityFlow<TActivityReturns> SetParent(Activity parent)
        {
            _parent = parent;
            return this;
        }

        public IActivityFlow<TActivityReturns> SetPrevious(Activity previous)
        {
            _previous = previous;
            return this;
        }
        
        /// <inheritdoc/>
        public ActivityAction<TActivityReturns> Action()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction<TActivityReturns>(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityCondition<bool> If()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityInstance = new ActivityCondition<bool>(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityInstance = new ActivityLoopUntilTrue<TActivityReturns>(activityInformation, _asyncRequestClient , _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TActivityReturns, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachParallel<TActivityReturns, TItem>(activityInformation, _asyncRequestClient, items, _previous, _parent);
            return activityInstance;
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachSequential<TActivityReturns, TItem>(activityInformation, _asyncRequestClient, items, _previous, _parent);
            return activityInstance;
        }
    }
}