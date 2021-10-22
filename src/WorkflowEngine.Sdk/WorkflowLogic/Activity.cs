using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public delegate Task<TMethodReturnType> ActivityMethod<TMethodReturnType>(
        Activity activity,
        CancellationToken cancellationToken);
    public delegate Task ActivityMethod(
        Activity activity,
        CancellationToken cancellationToken);

    public abstract class Activity
    {
        public IActivityExecutor ActivityExecutor { get; }

        protected internal ActivityInformation ActivityInformation { get; }
        public int? Iteration { get; protected set; }

        public string Title
        {
            get
            {
                if (!NestedIterations.Any()) return ActivityInformation.NestedPositionAndTitle;
                var iterations = string.Join(",", NestedIterations);
                return $"{ActivityInformation.NestedPositionAndTitle} [{iterations}]";
            }
        }

        /// <inheritdoc />
        public override string ToString() => Title;

        public List<int> NestedIterations { get; } = new();

        protected Activity(ActivityInformation activityInformation,
            IActivityExecutor activityExecutor)
        {
            InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));

            ActivityExecutor = activityExecutor;
            ActivityExecutor.Activity = this;
            
            ActivityInformation = activityInformation;

            activityInformation.MethodHandler.InstanceTitle = activityInformation.NestedPositionAndTitle;
            if (AsyncWorkflowStatic.Context.ParentActivityInstanceId != null)
            {
                var parentActivity =
                    ActivityInformation.WorkflowInformation.GetActivity(AsyncWorkflowStatic.Context
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
            return ActivityInformation.MethodHandler.GetArgument<TParameter>(parameterName);
        }
    }
}