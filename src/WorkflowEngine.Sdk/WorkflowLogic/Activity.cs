using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        protected Activity ParentActivity { get; }
        // TODO: Should be nullable instead of relying on value 0
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
            IActivityExecutor activityExecutor,
            Activity parentActivity)
        {
            InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));

            ActivityExecutor = activityExecutor;
            
            ActivityInformation = activityInformation;
            ParentActivity = parentActivity;

            activityInformation.MethodHandler.InstanceTitle = activityInformation.NestedPositionAndTitle;
            if (ParentActivity != null)
            {
                NestedIterations.AddRange(ParentActivity.NestedIterations);
                if (ParentActivity.Iteration is > 0)
                {
                    NestedIterations.Add(ParentActivity.Iteration.Value);
                }
            }
        }
    }
}