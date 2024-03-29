﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
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
    public async Task<TActivityReturns> ExecuteAsync(
        ActivityMethodAsync<IActivityCondition<TActivityReturns>, TActivityReturns> methodAsync,
        CancellationToken cancellationToken = default)
    {
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync( ct => LogicExecutor.ExecuteWithReturnValueAsync(t => methodAsync(this, t), "Condition", ct),
            DefaultValueMethodAsync, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public async Task<TActivityReturns> ExecuteAsync(ActivityMethod<IActivityCondition<TActivityReturns>, TActivityReturns> method, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync( (a, _) => Task.FromResult(method(a)), cancellationToken);
        return result;
    }
}