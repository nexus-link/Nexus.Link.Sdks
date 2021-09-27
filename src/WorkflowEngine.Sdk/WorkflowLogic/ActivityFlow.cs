using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    internal class ActivityFlow : IActivityFlow
    {
        private readonly WorkflowInformation _workflowInformation;
        private readonly IWorkflowCapability _workflowCapability;
        private readonly string _activityFormId;
        private MethodHandler _methodHandler;
        private readonly string _formTitle;
        private Activity _parent;
        private Activity _previous;

        public ActivityFlow(IWorkflowCapability workflowCapability, WorkflowInformation workflowInformation, string formTitle, string activityFormId)
        {
            _workflowInformation = workflowInformation;
            _workflowCapability = workflowCapability;
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

        public IActivityFlow SetParent(Activity parent, bool alsoSetAsPrevious = false)
        {
            _parent = parent;
            if (alsoSetAsPrevious) SetPrevious(parent);
            return this;
        }

        public IActivityFlow SetPrevious(Activity previous)
        {
            _previous = previous;
            return this;
        }

        public Task<TMethodReturnType> ExecuteActionAsync<TMethodReturnType>(
            Func<ActivityAction, CancellationToken, Task<TMethodReturnType>> method,
            CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction(_workflowCapability, activityInformation, _previous, _parent);
            return activityInstance.ExecuteActionAsync(method, cancellationToken);
        }

        public Task ExecuteActionAsync(
            Func<ActivityAction, CancellationToken, Task> method,
            CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.Action);
            var activityInstance = new ActivityAction(_workflowCapability, activityInformation, _previous, _parent);
            return activityInstance.ExecuteActionAsync(method, cancellationToken);
        }

        public Task<bool> IfAsync(
            Func<Activity, CancellationToken, Task<bool>> ifMethodAsync,
            CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.IfThenElse);
            var activityInstance = new ActivityIf(_workflowCapability, activityInformation, _previous, _parent);
            return activityInstance.IfAsync(ifMethodAsync, cancellationToken);
        }

        public Task<TMethodReturnType> LoopUntilTrueAsync<TMethodReturnType>(
            Func<ActivityLoopUntilTrue, CancellationToken, Task<TMethodReturnType>> methodAsync, CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.LoopUntilTrue);
            var activityInstance = new ActivityLoopUntilTrue(_workflowCapability, activityInformation, _previous, _parent);
            return activityInstance.ExecuteAsync<TMethodReturnType>(methodAsync, cancellationToken);
        }

        public Task ForEachParallelAsync<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, ActivityForEachParallel<TItem>, CancellationToken, Task> methodAsync, CancellationToken cancellationToken)
        {
            var activityInformation = CreateActivityInformation(WorkflowActivityTypeEnum.ForEachParallel);
            var activityInstance = new ActivityForEachParallel<TItem>(_workflowCapability, activityInformation, items, _previous, _parent);
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