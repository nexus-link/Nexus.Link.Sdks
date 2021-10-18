using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityIf<TActivityReturns> : ActivityCondition<TActivityReturns>
    {
        /// <inheritdoc />
        public ActivityIf(ActivityInformation activityInformation, IAsyncRequestClient asyncRequestClient, Activity previousActivity, Activity parentActivity, Func<Task<TActivityReturns>> getDefaultValueMethodAsync)
            : base(activityInformation, asyncRequestClient, previousActivity, parentActivity, getDefaultValueMethodAsync)
        {
            InternalContract.Require(typeof(TActivityReturns) == typeof(bool), $"You can only use {nameof(IActivityFlow<TActivityReturns>.If)}() with type {nameof(Boolean)}, not with type {nameof(TActivityReturns)}." );
        }
    }
}