using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.ActivityLogic
{
    public class ActivityAction : Activity
    {
        public ActivityAction(IInternalActivityFlow activityFlow)
            : base(ActivityTypeEnum.Action, activityFlow)
        {
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

        public ActivityAction(
            IInternalActivityFlow activityFlow, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(ActivityTypeEnum.Action, activityFlow)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
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