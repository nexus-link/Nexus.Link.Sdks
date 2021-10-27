using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic.Activities
{
    public class ActivityAction : Activity
    {
        public ActivityAction(ActivityPersistence activityPersistence, IWorkflowVersion workflowVersion)
            : base(activityPersistence, workflowVersion)
        {
            InternalContract.RequireAreEqual(ActivityTypeEnum.Action, ActivityPersistence.ActivityType, "Ignore",
                $"The activity {ActivityPersistence} was declared as {ActivityPersistence.ActivityType}, so you can't use {nameof(ActivityAction)}.");
        }
        
        public Task ExecuteAsync(
            Func<ActivityAction, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((instance, t) => MapMethodAsync(method, instance, t), cancellationToken);
        }

        private Task MapMethodAsync(
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
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityAction(ActivityPersistence activityPersistence,
            IWorkflowVersion workflowVersion, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityPersistence, workflowVersion)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
            InternalContract.RequireAreEqual(ActivityTypeEnum.Action, ActivityPersistence.ActivityType, "Ignore",
                $"The activity {ActivityPersistence} was declared as {ActivityPersistence.ActivityType}, so you can't use {nameof(ActivityAction)}.");
        }
        public Task<TActivityReturns> ExecuteAsync(
            Func<ActivityAction<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, 
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((instance, t) => MapMethodAsync(method, instance, t), _getDefaultValueMethodAsync, cancellationToken);
        }

        private Task<TActivityReturns> MapMethodAsync(
            Func<ActivityAction<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, 
            Activity instance, CancellationToken cancellationToken)
        {
            var action = instance as ActivityAction<TActivityReturns>;
            FulcrumAssert.IsNotNull(action, CodeLocation.AsString());
            return method(action, cancellationToken);
        }
    }
}