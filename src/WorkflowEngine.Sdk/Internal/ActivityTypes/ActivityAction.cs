using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityAction" />
internal class ActivityAction : Activity, IActivityAction
{
    public ActivityAction(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    /// <inheritdoc/>
    public Task ExecuteAsync(ActivityActionMethodAsync methodAsync, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync( ct => methodAsync(this, ct), cancellationToken);
    }
}

internal class ActivityAction<TActivityReturns> : Activity<TActivityReturns>, IActivityAction<TActivityReturns>
{
    public ActivityAction(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueMethodAsync = null)
        : base(activityInformation, getDefaultValueMethodAsync)
    {
    }

    /// <inheritdoc/>
    public Task<TActivityReturns> ExecuteAsync(ActivityActionMethodAsync<TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync( ct => methodAsync(this, ct), GetDefaultValueMethodAsync, cancellationToken);
    }
}