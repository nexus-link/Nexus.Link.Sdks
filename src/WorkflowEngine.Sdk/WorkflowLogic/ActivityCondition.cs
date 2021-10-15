using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityCondition<TMethodReturnType> : Activity
    {
        public ActivityCondition(ActivityInformation activityInformation, IAsyncRequestClient asyncRequestClient,
            Activity previousActivity, Activity parentActivity, object getDefaultValueMethodAsync)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.Condition, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityCondition<TMethodReturnType>)}.");
        }

        public Task<TMethodReturnType> ExecuteAsync(
            Func<Activity, CancellationToken, Task<TMethodReturnType>> conditionMethodAsync,
            CancellationToken cancellationToken)
        {
            return InternalExecuteAsync((instance, ct) => MapMethod(conditionMethodAsync, instance, ct), cancellationToken);
        }

        private static Task<TMethodReturnType> MapMethod(
            Func<ActivityCondition<TMethodReturnType>, CancellationToken, Task<TMethodReturnType>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var condition = instance as ActivityCondition<TMethodReturnType>;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }
    }
}