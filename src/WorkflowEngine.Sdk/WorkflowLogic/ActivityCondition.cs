using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityCondition<TActivityReturns> : Activity
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityCondition(ActivityInformation activityInformation, IActivityExecutor activityExecutor, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, activityExecutor)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.Condition, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityCondition<TActivityReturns>)}.");
        }

        public Task<TActivityReturns> ExecuteAsync(
            Func<Activity, CancellationToken, Task<TActivityReturns>> conditionMethodAsync,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteAsync((instance, ct) => MapMethodAsync(conditionMethodAsync, instance, ct), _getDefaultValueMethodAsync, cancellationToken);
        }

        private static Task<TActivityReturns> MapMethodAsync(
            Func<ActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var condition = instance as ActivityCondition<TActivityReturns>;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }
    }
}