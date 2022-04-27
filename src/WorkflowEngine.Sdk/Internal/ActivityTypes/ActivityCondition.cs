using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityCondition{TActivityReturns}" />
[Obsolete("Please use ActivityIf. Obsolete since 2022-04-27.")]
internal class ActivityCondition<TActivityReturns> : Activity<TActivityReturns>, IActivityCondition<TActivityReturns>
{

    public ActivityCondition(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueMethodAsync)
        : base(activityInformation, getDefaultValueMethodAsync)
    {
    }

    /// <inheritdoc/>
    public Task<TActivityReturns> ExecuteAsync(
        ActivityConditionMethodAsync<TActivityReturns> methodAsync,
        CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync( ct => methodAsync(this, ct), GetDefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<TActivityReturns> ExecuteAsync(ActivityConditionMethod<TActivityReturns> method, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync( _ => Task.FromResult(method(this)), GetDefaultValueMethodAsync, cancellationToken);
    }
}