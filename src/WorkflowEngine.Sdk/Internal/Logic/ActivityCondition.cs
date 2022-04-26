using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityCondition<TActivityReturns> : Activity, IActivityCondition<TActivityReturns>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityCondition(IActivityInformation activityInformation, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
        }

        public Task<TActivityReturns> ExecuteAsync(
            Func<IActivityCondition<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteWithReturnValueAsync( ct => method(this, ct), _getDefaultValueMethodAsync, cancellationToken);
        }

        public Task<TActivityReturns> ExecuteAsync(Func<IActivityCondition<TActivityReturns>, TActivityReturns> method,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteWithReturnValueAsync( _ => Task.FromResult(method(this)), _getDefaultValueMethodAsync, cancellationToken);
        }
    }
}