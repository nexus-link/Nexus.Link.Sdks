using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="ActivityForEachSequential{TItem}" />
internal class ActivityForEachSequential<TItem> : LoopActivity, IActivityForEachSequential<TItem>
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
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TItem> methodAsync,
        CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivity = this;
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => ForEachSequentialAsync(methodAsync, ct),
            cancellationToken);
    }

    internal Task ForEachSequentialAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        return ForEachSequentialAsync(_methodAsync, cancellationToken);
    }

    private async Task ForEachSequentialAsync(ActivityForEachSequentialMethodAsync<TItem> method, CancellationToken cancellationToken)
    {
        foreach (var item in Items)
        {
            LoopIteration++;
            await method(item, this, cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);

        }
    }

    /// <inheritdoc />
    protected override async Task InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ForEachSequentialAsync, cancellationToken);
    }
}

/// <inheritdoc cref="ActivityForEachSequential{TMethodReturns, TItem}" />
internal class ActivityForEachSequential<TMethodReturns, TItem> :
    LoopActivity<IList<TMethodReturns>, TMethodReturns>, IActivityForEachSequential<TMethodReturns, TItem>
{
    private readonly ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> _methodAsync;
    public IEnumerable<TItem> Items { get; }

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityForEachSequential(
        IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync, IEnumerable<TItem> items)
        : base(activityInformation, getDefaultValueAsync)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        Items = items;
        ChildCounter = 0;
    }
    public ActivityForEachSequential(
        IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync,
        IEnumerable<TItem> items,
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync)
        : base(activityInformation, getDefaultValueAsync)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
        Items = items;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task<IList<TMethodReturns>> ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> method,
        CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivity = this;
        return ActivityExecutor.ExecuteWithReturnValueAsync(
            ct => ForEachSequentialAsync(method, ct),
            _ => Task.FromResult((IList<TMethodReturns>)new List<TMethodReturns>()),
            cancellationToken);
    }

    internal Task<IList<TMethodReturns>> ForEachSequentialAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        return ForEachSequentialAsync(_methodAsync, cancellationToken);
    }

    private async Task<IList<TMethodReturns>> ForEachSequentialAsync(
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync,
        CancellationToken cancellationToken)
    {
        var resultList = new List<TMethodReturns>();
        foreach (var item in Items)
        {
            LoopIteration++;
            var result = await methodAsync(item, this, cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);
            resultList.Add(result);
        }

        return resultList;
    }

    /// <inheritdoc />
    protected override async Task<IList<TMethodReturns>> InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync(
            ct => ForEachSequentialAsync(_methodAsync, ct),
            _ => Task.FromResult((IList<TMethodReturns>)new List<TMethodReturns>()),
            cancellationToken);
        return result;
    }
}