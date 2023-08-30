using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <summary>
/// Make the workflow sleep until a specific time.
/// </summary>
internal class ActivitySleep: Activity, IActivitySleep
{
    internal const string ContextSleepUntil = "SleepUntil";

    public ActivitySleep(IActivityInformation activityInformation, DateTimeOffset sleepUntil)
        : base(activityInformation)
    {
        SetInternalContext(ContextSleepUntil, sleepUntil);
    }

    public ActivitySleep(IActivityInformation activityInformation, TimeSpan timeToSleep)
        : base(activityInformation)
    {
        if (TryGetInternalContext<DateTimeOffset>(ContextSleepUntil, out var sleepUntil)) return;
        sleepUntil = DateTimeOffset.UtcNow.Add(timeToSleep);
        SetInternalContext(ContextSleepUntil, sleepUntil);
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(SleepAsync, cancellationToken);
    }

    internal async Task SleepAsync(CancellationToken cancellationToken = default)
    {
        var success = TryGetInternalContext<DateTimeOffset>(ContextSleepUntil, out var sleepUntil);
        FulcrumAssert.IsTrue(success, CodeLocation.AsString());
        if (sleepUntil >= DateTimeOffset.UtcNow)
        {
            throw new RequestPostponedException
            {
                TryAgain = true,
                TryAgainAfterMinimumTimeSpan = sleepUntil.Subtract(DateTimeOffset.UtcNow)
            };
        }

        await Task.CompletedTask;
    }
}