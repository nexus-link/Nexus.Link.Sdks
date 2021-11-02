using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.ActivityLogic
{
    public delegate Task<TMethodReturnType> ActivityMethod<TMethodReturnType>(
        Activity activity,
        CancellationToken cancellationToken);
    public delegate Task ActivityMethod(
        Activity activity,
        CancellationToken cancellationToken);

    public abstract class Activity
    {
        private readonly IInternalActivityFlow _activityFlow;
        private readonly ActivityExecutor _activityExecutor;

        protected internal WorkflowCache WorkflowCache => _activityFlow.WorkflowCache;
        
        protected internal ActivityForm Form => WorkflowCache.GetActivityForm(_activityFlow.ActivityFormId);
        protected internal ActivityVersion Version => WorkflowCache.GetActivityVersionByFormId(_activityFlow.ActivityFormId);
        protected internal ActivityInstance Instance => WorkflowCache.GetActivityInstance(InstanceId);
        public int? Iteration { get; protected set; }
        public string NestedPosition { get; }
        public string NestedPositionAndTitle => $"{NestedPosition} {_activityFlow.FormTitle}";

        public string Title
        {
            get
            {
                if (!NestedIterations.Any()) return NestedPositionAndTitle;
                var iterations = string.Join(",", NestedIterations);
                return $"{NestedPositionAndTitle} [{iterations}]";
            }
        }

        /// <inheritdoc />
        public override string ToString() => Title;

        public List<int> NestedIterations { get; } = new();

        public string InstanceId { get; internal set; }

        protected Activity(ActivityTypeEnum activityType, IInternalActivityFlow activityFlow)
        {
            InternalContract.RequireNotNull(activityFlow, nameof(activityFlow));
            _activityFlow = activityFlow;
            Activity parentActivity = null;
            if (AsyncWorkflowStatic.Context.ParentActivityInstanceId != null)
            {
                parentActivity = WorkflowCache.GetActivity(AsyncWorkflowStatic.Context.ParentActivityInstanceId);
                FulcrumAssert.IsNotNull(parentActivity, CodeLocation.AsString());
                NestedIterations.AddRange(parentActivity.NestedIterations);
                if (parentActivity.Iteration is > 0)
                {
                    NestedIterations.Add(parentActivity.Iteration.Value);
                }
                NestedPosition = $"{parentActivity.NestedPosition}.{activityFlow.Position}";
            }
            else
            {
                NestedPosition = $"{activityFlow.Position}";
            }

            InstanceId = _activityFlow.WorkflowCache.GetOrCreateInstanceId(activityType, _activityFlow, parentActivity);
            _activityExecutor = new ActivityExecutor(activityFlow.WorkflowVersion, this);
        }

        public TParameter GetArgument<TParameter>(string parameterName)
        {
            return _activityFlow.MethodHandler.GetArgument<TParameter>(parameterName);
        }

        internal Task<TMethodReturnType> InternalExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            return _activityExecutor.ExecuteAsync(method, getDefaultValueMethodAsync, cancellationToken);
        }

        internal async Task InternalExecuteAsync(
            ActivityMethod method,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            await _activityExecutor.ExecuteAsync(method, cancellationToken);
        }
    }
}