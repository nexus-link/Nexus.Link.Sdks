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

    public ActivityCondition(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
    }

    /// <inheritdoc/>
    public Task<TActivityReturns> ExecuteAsync(
        ActivityMethodAsync<IActivityCondition<TActivityReturns>, TActivityReturns> methodAsync,
        CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync( ct => LogicExecutor.ExecuteWithReturnValueAsync(t => methodAsync(this, t), "Condition", ct),
            DefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<TActivityReturns> ExecuteAsync(ActivityMethod<IActivityCondition<TActivityReturns>, TActivityReturns> method, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync( (a, _) => Task.FromResult(method(a)), cancellationToken);
    }
}