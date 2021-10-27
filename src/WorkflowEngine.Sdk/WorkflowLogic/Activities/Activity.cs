using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic.Activities
{
    public delegate Task<TMethodReturnType> ActivityMethod<TMethodReturnType>(
        Activity activity,
        CancellationToken cancellationToken);
    public delegate Task ActivityMethod(
        Activity activity,
        CancellationToken cancellationToken);

    public abstract class Activity
    {
        private ActivityExecutor _activityExecutor;

        protected internal ActivityPersistence ActivityPersistence { get; }
        public int? Iteration { get; protected set; }

        public string Title
        {
            get
            {
                if (!NestedIterations.Any()) return ActivityPersistence.NestedPositionAndTitle;
                var iterations = string.Join(",", NestedIterations);
                return $"{ActivityPersistence.NestedPositionAndTitle} [{iterations}]";
            }
        }

        /// <inheritdoc />
        public override string ToString() => Title;

        public List<int> NestedIterations { get; } = new();

        protected Activity(ActivityPersistence activityPersistence, IWorkflowVersion workflowVersion)
        {
            InternalContract.RequireNotNull(activityPersistence, nameof(activityPersistence));
            InternalContract.RequireNotNull(workflowVersion, nameof(workflowVersion));
            
            ActivityPersistence = activityPersistence;
            _activityExecutor = new ActivityExecutor(workflowVersion, this);

            activityPersistence.MethodHandler.InstanceTitle = activityPersistence.NestedPositionAndTitle;
            if (AsyncWorkflowStatic.Context.ParentActivityInstanceId != null)
            {
                var parentActivity =
                    ActivityPersistence.WorkflowPersistence.GetActivity(AsyncWorkflowStatic.Context
                        .ParentActivityInstanceId);
                NestedIterations.AddRange(parentActivity.NestedIterations);
                if (parentActivity.Iteration is > 0)
                {
                    NestedIterations.Add(parentActivity.Iteration.Value);
                }
            }
        }

        public TParameter GetArgument<TParameter>(string parameterName)
        {
            return ActivityPersistence.MethodHandler.GetArgument<TParameter>(parameterName);
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