using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityAction : Activity
    {
        public ActivityAction(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            ActivityInformation activityInformation, 
            Activity previousActivity, Activity parentActivity)
            :base(workflowCapability, asyncRequestClient, activityInformation, previousActivity, parentActivity)
        {
            InternalContract.RequireAreEqual(WorkflowActivityTypeEnum.Action, ActivityInformation.ActivityType, "Ignore",
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't use {nameof(ActivityAction)}.");
        }

        public Task<TMethodReturnType> ExecuteActionAsync<TMethodReturnType>(
            Func<ActivityAction, CancellationToken, Task<TMethodReturnType>> method, 
            CancellationToken cancellationToken)
        {
            InternalContract.Require(ActivityInformation.ActivityType == WorkflowActivityTypeEnum.Action || ActivityInformation.ActivityType == WorkflowActivityTypeEnum.LoopUntilTrue,
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't call {nameof(ExecuteActionAsync)}.");
            
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

        public Task ExecuteActionAsync(
            Func<ActivityAction, CancellationToken, Task> method,
            CancellationToken cancellationToken, params object[] arguments)
        {
            InternalContract.Require(ActivityInformation.ActivityType == WorkflowActivityTypeEnum.Action || ActivityInformation.ActivityType == WorkflowActivityTypeEnum.LoopUntilTrue,
                $"The activity {ActivityInformation} was declared as {ActivityInformation.ActivityType}, so you can't call {nameof(ExecuteActionAsync)}.");
            
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
}