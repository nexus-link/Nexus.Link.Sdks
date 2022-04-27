using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="ActivityForEachSequential{TItem}" />
internal class ActivityForEachSequential<TItem> : Activity, IActivityForEachSequential<TItem>
{
    public IEnumerable<TItem> Items { get; }

    public ActivityForEachSequential(
        IActivityInformation activityInformation, IEnumerable<TItem> items)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        Items = items;
        Iteration = 0;
    }

    /// <inheritdoc/>
    public Task ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TItem> methodAsync,
        CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => ForEachMethod(methodAsync, ct),
            cancellationToken);
    }

    private async Task ForEachMethod(ActivityForEachSequentialMethodAsync<TItem> method, CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        foreach (var item in Items)
        {
            Iteration++;
            await method(item, this, cancellationToken);
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            ActivityInformation.Workflow.LatestActivity = this;
        }
    }
}

/// <inheritdoc cref="ActivityForEachSequential{TMethodReturns, TItem}" />
internal class ActivityForEachSequential<TMethodReturns, TItem> : Activity, IActivityForEachSequential<TMethodReturns, TItem>
{
    private readonly ActivityDefaultValueMethodAsync<TMethodReturns> _getDefaultValueAsync;
    public IEnumerable<TItem> Items { get; }

    public ActivityForEachSequential(
        IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync, IEnumerable<TItem> items)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        _getDefaultValueAsync = getDefaultValueAsync;
        Items = items;
        Iteration = 0;
    }  
    
    /// <inheritdoc/>
    public Task<IList<TMethodReturns>> ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> method,
        CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(ct => ForEachMethod(method, ct), _ => Task.FromResult((IList<TMethodReturns>) new List<TMethodReturns>()), cancellationToken);
    }

    private async Task<IList<TMethodReturns>> ForEachMethod(ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync,
        CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        var resultList = new List<TMethodReturns>();
        foreach (var item in Items)
        {
            Iteration++;
            var result = await methodAsync(item, this, cancellationToken);
            resultList.Add(result);
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            ActivityInformation.Workflow.LatestActivity = this;
        }

        return resultList;
    }
}