using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivitySleep: Activity, IActivitySleep
    {
        public ActivitySleep(IInternalActivityFlow activityFlow)
            : base(ActivityTypeEnum.Action, activityFlow)
        {
        }

        public Task ExecuteAsync(TimeSpan timeToSleep, CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync((_, _) => Task.FromException(new FulcrumTryAgainException
            {
                RecommendedWaitTimeInSeconds = timeToSleep.TotalSeconds
            }), cancellationToken);
        }
    }
}