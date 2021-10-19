using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityIf<TActivityReturns> : ActivityCondition<TActivityReturns>
    {
        /// <inheritdoc />
        public ActivityIf(ActivityInformation activityInformation, IActivityExecutor activityExecutor, Activity parentActivity, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, activityExecutor, parentActivity, getDefaultValueMethodAsync)
        {
            InternalContract.Require(typeof(TActivityReturns) == typeof(bool), $"You can only use {nameof(IActivityFlow<TActivityReturns>.If)}() with type {nameof(Boolean)}, not with type {nameof(TActivityReturns)}." );
        }
    }
}