using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <summary>
    /// Make the workflow sleep for a time span of <see cref="TimeToSleep"/>.
    /// </summary>
    internal class ActivitySleep: Activity, IActivitySleep
    {
        public TimeSpan TimeToSleep { get; }

        public ActivitySleep(IActivityInformation activityInformation, TimeSpan timeToSleep)
            : base(activityInformation)
        {
            TimeToSleep = timeToSleep;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (Instance.HasCompleted) return;
            var now = DateTimeOffset.UtcNow;
            var sleepUntil = await InternalExecuteAsync((_, _) => Task.FromResult(now.Add(TimeToSleep)), null, cancellationToken);
            if (sleepUntil <= now) return;
            throw new ExceptionTransporter(new RequestPostponedException
            {
                TryAgain = true,
                TryAgainAfterMinimumTimeSpan = sleepUntil.Subtract(now)
            });
        }
    }
}