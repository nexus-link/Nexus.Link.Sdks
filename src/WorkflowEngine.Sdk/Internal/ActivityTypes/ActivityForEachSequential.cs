using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="ActivityForEachSequential{TItem}" />
internal class ActivityForEachSequential<TItem> :
    Activity, IActivityForEachSequential<TItem>, IBackgroundActivity
{
    private readonly ActivityForEachSequentialMethodAsync<TItem> _methodAsync;
    public IEnumerable<TItem> Items { get; }

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityForEachSequential(
        IActivityInformation activityInformation, IEnumerable<TItem> items)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        Items = items;
        Iteration = 0;
    }
    public ActivityForEachSequential(
        IActivityInformation activityInformation, IEnumerable<TItem> items,
        ActivityForEachSequentialMethodAsync<TItem> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
        Items = items;
        Iteration = 0;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TItem> methodAsync,
        CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => ForEachMethod(methodAsync, ct),
            cancellationToken);
    }

    internal async Task ForEachMethod(ActivityForEachSequentialMethodAsync<TItem> method, CancellationToken cancellationToken)
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

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => ForEachMethod(_methodAsync, ct),
            cancellationToken);
    }
}

/// <inheritdoc cref="ActivityForEachSequential{TMethodReturns, TItem}" />
internal class ActivityForEachSequential<TMethodReturns, TItem> :
    Activity, IActivityForEachSequential<TMethodReturns, TItem>,
    IBackgroundActivity<IList<TMethodReturns>>
{
    private readonly ActivityDefaultValueMethodAsync<TMethodReturns> _getDefaultValueAsync;
    private readonly ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> _methodAsync;
    public IEnumerable<TItem> Items { get; }

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityForEachSequential(
        IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync, IEnumerable<TItem> items)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        _getDefaultValueAsync = getDefaultValueAsync;
        Items = items;
        Iteration = 0;
    }
    public ActivityForEachSequential(
        IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync,
        IEnumerable<TItem> items,
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _getDefaultValueAsync = getDefaultValueAsync;
        _methodAsync = methodAsync;
        Items = items;
        Iteration = 0;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task<IList<TMethodReturns>> ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> method,
        CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(
            ct => ForEachMethod(method, ct),
            _ => Task.FromResult((IList<TMethodReturns>)new List<TMethodReturns>()),
            cancellationToken);
    }

    internal async Task<IList<TMethodReturns>> ForEachMethod(
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync,
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

    /// <inheritdoc />
    public Task<IList<TMethodReturns>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(
            ct => ForEachMethod(_methodAsync, ct),
            _ => Task.FromResult((IList<TMethodReturns>)new List<TMethodReturns>()),
            cancellationToken);
    }
}