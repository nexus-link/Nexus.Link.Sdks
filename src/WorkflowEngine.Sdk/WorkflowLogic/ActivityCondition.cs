using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityCondition<TActivityReturns> : Activity
    {
        private readonly Func<Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityCondition(ActivityInformation activityInformation, IAsyncRequestClient asyncRequestClient, Activity parentActivity, Func<Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, asyncRequestClient, parentActivity)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.Condition, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityCondition<TActivityReturns>)}.");
        }

        public Task<TActivityReturns> ExecuteAsync(
            Func<Activity, CancellationToken, Task<TActivityReturns>> conditionMethodAsync,
            CancellationToken cancellationToken)
        {
            return InternalExecuteAsync((instance, ct) => MapMethod(conditionMethodAsync, instance, ct), _getDefaultValueMethodAsync, cancellationToken);
        }

        private static Task<TActivityReturns> MapMethod(
            Func<ActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var condition = instance as ActivityCondition<TActivityReturns>;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }
    }
}