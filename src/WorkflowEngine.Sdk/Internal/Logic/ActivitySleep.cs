using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <summary>
    /// Make the workflow sleep for a time span of <see cref="TimeToSleep"/>.
    /// </summary>
    internal class ActivitySleep: Activity, IActivitySleep
    {
        public TimeSpan TimeToSleep { get; }

        public ActivitySleep(IInternalActivityFlow activityFlow, TimeSpan timeToSleep)
            : base(ActivityTypeEnum.Action, activityFlow)
        {
            TimeToSleep = timeToSleep;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (Instance.HasCompleted) return;
            await InternalExecuteAsync((_, _) => Task.CompletedTask, cancellationToken);
            throw new ExceptionTransporter(new RequestPostponedException
            {
                TryAgain = true,
                TryAgainAfterMinimumTimeSpan = TimeToSleep
            });
        }
    }
}