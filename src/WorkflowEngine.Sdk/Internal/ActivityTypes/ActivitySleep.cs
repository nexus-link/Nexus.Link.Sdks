using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

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

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (Instance.HasCompleted) return;
        var now = DateTimeOffset.UtcNow;
        var sleepUntil = await ActivityExecutor.ExecuteWithReturnValueAsync(_ => Task.FromResult(now.Add(TimeToSleep)), null, cancellationToken);
        if (sleepUntil <= now) return;
        throw new WorkflowImplementationShouldNotCatchThisException(new RequestPostponedException
        {
            TryAgain = true,
            TryAgainAfterMinimumTimeSpan = sleepUntil.Subtract(now)
        });
    }
}