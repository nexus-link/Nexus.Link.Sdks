using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    public class ActivityCondition<TActivityReturns> : Activity<TActivityReturns>, IActivityCondition<TActivityReturns>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityCondition(IInternalActivityFlow activityFlow, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(ActivityTypeEnum.Condition, activityFlow)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
        }

        public Task<TActivityReturns> ExecuteAsync(
            Func<IActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> conditionMethodAsync,
            CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((instance, ct) => MapMethodAsync(conditionMethodAsync, instance, ct), _getDefaultValueMethodAsync, cancellationToken);
        }

        private static Task<TActivityReturns> MapMethodAsync(
            Func<IActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            Activity instance, CancellationToken cancellationToken)
        {
            var condition = instance as IActivityCondition<TActivityReturns>;
            FulcrumAssert.IsNotNull(condition, CodeLocation.AsString());
            return method(condition, cancellationToken);
        }
    }
}