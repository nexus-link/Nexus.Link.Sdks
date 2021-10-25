using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityIf<TActivityReturns> : ActivityCondition<TActivityReturns>
    {
        /// <inheritdoc />
        public ActivityIf(ActivityPersistence activityPersistence, IActivityExecutor activityExecutor, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityPersistence, activityExecutor, getDefaultValueMethodAsync)
        {
            InternalContract.Require(typeof(TActivityReturns) == typeof(bool), $"You can only use {nameof(IActivityFlow<TActivityReturns>.If)}() with type {nameof(Boolean)}, not with type {nameof(TActivityReturns)}." );
        }
    }
}