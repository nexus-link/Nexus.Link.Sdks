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
    internal class ActivityAction : Activity, IActivityAction
    {
        public ActivityAction(IActivityInformation activityInformation)
            : base(activityInformation)
        {
        }

        public Task ExecuteAsync(
            Func<IActivityAction, CancellationToken, Task> method,
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteWithoutReturnValueAsync( ct => method(this, ct), cancellationToken);
        }
    }

    internal class ActivityAction<TActivityReturns> : Activity, IActivityAction<TActivityReturns>
    {
        private readonly Func<CancellationToken, Task<TActivityReturns>> _getDefaultValueMethodAsync;

        public ActivityAction(IActivityInformation activityInformation, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation)
        {
            _getDefaultValueMethodAsync = getDefaultValueMethodAsync;
        }
        public Task<TActivityReturns> ExecuteAsync(
            Func<IActivityAction<TActivityReturns>, CancellationToken, Task<TActivityReturns>> method, 
            CancellationToken cancellationToken = default)
        {
            return ActivityExecutor.ExecuteWithReturnValueAsync( ct => method(this, ct), _getDefaultValueMethodAsync, cancellationToken);
        }
    }
}