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
    internal class ActivityFlow : IActivityFlow
    {
        private readonly WorkflowInformation _workflowInformation;
        private readonly IWorkflowCapability _workflowCapability;
        private readonly IAsyncRequestClient _asyncRequestClient;
        private readonly string _activityFormId;
        private readonly MethodHandler _methodHandler;
        private readonly string _formTitle;
        private Activity _parent;
        private Activity _previous;

        public ActivityFlow(IWorkflowCapability workflowCapability,
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

        #region Action
        /// <inheritdoc/>
        public ActivityAction Action()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance;
        }

        /// <inheritdoc/>
        [Obsolete("Use Action().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        public Task<TMethodReturnType> ExecuteActionAsync<TMethodReturnType>(
            Func<ActivityAction, CancellationToken, Task<TMethodReturnType>> method,
            CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance.ExecuteActionAsync(method, cancellationToken);
        }

        /// <inheritdoc/>
        [Obsolete("Use Action().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        public Task ExecuteActionAsync(
            Func<ActivityAction, CancellationToken, Task> method,
            CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance.ExecuteActionAsync(method, cancellationToken);
        }
        #endregion

        #region If
        public ActivityCondition<bool> If()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityInstance = new ActivityCondition<bool>(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance;
        }
        public ActivityCondition<T> Condition<T>()
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityInstance = new ActivityCondition<T>(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance;
        }
        
        [Obsolete("Use If().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        public Task<bool> IfAsync(
            Func<Activity, CancellationToken, Task<bool>> ifMethodAsync,
            CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Condition);
            var activityInstance = new ActivityIf(activityInformation, _asyncRequestClient, _previous, _parent);
            return activityInstance.IfAsync(ifMethodAsync, cancellationToken);
        }
        #endregion

        public Task<TMethodReturnType> LoopUntilTrueAsync<TMethodReturnType>(
            Func<ActivityLoopUntilTrue, CancellationToken, Task<TMethodReturnType>> methodAsync, CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityInstance = new ActivityLoopUntilTrue(activityInformation, _asyncRequestClient , _previous, _parent);
            return activityInstance.ExecuteAsync<TMethodReturnType>(methodAsync, cancellationToken);
        }

        public Task ForEachParallelAsync<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, ActivityForEachParallel<TItem>, CancellationToken, Task> methodAsync, CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachParallel<TItem>(activityInformation, _asyncRequestClient, items, _previous, _parent);
            return activityInstance.ExecuteAsync(methodAsync, cancellationToken);
        }

        private ActivityInformation CreateActivityInformation(WorkflowActivityTypeEnum activityType)
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
}