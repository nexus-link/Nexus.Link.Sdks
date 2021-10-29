using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.ActivityLogic
{
    public class ActivityCondition<TActivityReturns> : Activity
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityCondition(IInternalActivityFlow activityFlow, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(ActivityTypeEnum.Condition, activityFlow)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
        }

        public Task<TActivityReturns> ExecuteAsync(
            Func<Activity, CancellationToken, Task<TActivityReturns>> conditionMethodAsync,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((instance, ct) => MapMethodAsync(conditionMethodAsync, instance, ct), _getDefaultValueMethodAsync, cancellationToken);
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