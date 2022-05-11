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
    internal const string ContextSleepUntil = "SleepUntil";
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
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(_ => SleepAsync(now), cancellationToken);
    }

    internal async Task SleepAsync(DateTimeOffset now)
    {
        var success = TryGetContext<DateTimeOffset>(ContextSleepUntil, out var sleepUntil);
        if (!success)
        {
            sleepUntil = now.Add(TimeToSleep);
            SetContext(ContextSleepUntil, sleepUntil);
        }

        if (sleepUntil > DateTimeOffset.UtcNow)
        {
            throw new RequestPostponedException
            {
                TryAgain = true,
                TryAgainAfterMinimumTimeSpan = sleepUntil.Subtract(now)
            };
        }

        await Task.CompletedTask;
    }
}