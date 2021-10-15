using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityIf : Activity
    {
        public ActivityIf(ActivityInformation activityInformation, IAsyncRequestClient asyncRequestClient,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.Condition, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityIf)}.");
        }
        
        [Obsolete("Use If().ExecuteAsync() instead. Obsolete since 2021-10-14.")]
        public Task<bool> IfAsync(
            Func<Activity, CancellationToken, Task<bool>> ifMethodAsync,
            CancellationToken cancellationToken)
        {
            return InternalExecuteAsync((instance, ct) => MapMethod(ifMethodAsync, instance, ct), cancellationToken);
        }

        private static Task<TMethodReturnType> MapMethod<TMethodReturnType>(
            Func<ActivityIf, CancellationToken, Task<TMethodReturnType>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var condition = instance as ActivityIf;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }
    }
}