using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityAction : Activity
    {
        public ActivityAction(ActivityInformation activityInformation,
            IAsyncRequestClient asyncRequestClient,
            Activity previousActivity, Activity parentActivity)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.Action, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityAction)}.");
        }
        
        public Task ExecuteAsync(
            Func<ActivityAction, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            return InternalExecuteAsync((instance, t) => MapMethod(method, instance, t), cancellationToken);
        }

        private Task MapMethod(
            Func<ActivityAction, CancellationToken, Task> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var action = instance as ActivityAction;
            FulcrumAssert.IsNotNull(action, CodeLocation.AsString());
            return method(action, cancellationToken);
        }
    }

    public class ActivityAction<TActivityReturns> : Activity
    {
        public ActivityAction(ActivityInformation activityInformation,
            IAsyncRequestClient asyncRequestClient,
            Activity previousActivity, Activity parentActivity, Func<Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.Action, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityAction)}.");
        }
        public Task<TActivityReturns> ExecuteAsync(
            Func<ActivityAction, CancellationToken, Task<TActivityReturns>> method, 
            CancellationToken cancellationToken)
        {
            return InternalExecuteAsync((instance, t) => MapMethod(method, instance, t), cancellationToken);
        }

        private Task<TMethodReturnType> MapMethod<TMethodReturnType>(
            Func<ActivityAction, CancellationToken, Task<TMethodReturnType>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var action = instance as ActivityAction;
            FulcrumAssert.IsNotNull(action, CodeLocation.AsString());
            return method(action, cancellationToken);
        }
    }
}