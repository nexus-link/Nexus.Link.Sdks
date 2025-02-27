﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="ActivityForEachSequential{TItem}" />
internal class ActivityForEachSequential<TItem> : LoopActivity, IActivityForEachSequential<TItem>
{
    private readonly GetIterationTitleMethod<TItem> _getIterationTitleMethod;
    private ActivityForEachSequentialMethodAsync<TItem> _methodAsync;
    public TItem[] Items { get; }

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityForEachSequential(
        IActivityInformation activityInformation, IEnumerable<TItem> items,
        GetIterationTitleMethod<TItem> getIterationTitleMethod)
        : base(activityInformation)
    {
        _getIterationTitleMethod = getIterationTitleMethod;
        InternalContract.RequireNotNull(items, nameof(items));
        Items = items.ToArray();
    }
    public ActivityForEachSequential(IActivityInformation activityInformation, IEnumerable<TItem> items,
        GetIterationTitleMethod<TItem> getIterationTitleMethod,
        ActivityForEachSequentialMethodAsync<TItem> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _getIterationTitleMethod = getIterationTitleMethod;
        _methodAsync = methodAsync;
        Items = items.ToArray();
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public async Task ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TItem> methodAsync,
        CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
        WorkflowStatic.Context.ParentActivity = this;
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ForEachSequentialAsync, cancellationToken);
        WorkflowStatic.Context.ParentActivity = null;
    }

    internal async Task ForEachSequentialAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        if (LoopIteration == 0) LoopIteration = 1;
        var index = LoopIteration - 1;
        while (index < Items.Length)
        {
            var item = Items[index];
            try
            {
                WorkflowStatic.Context.IterationTitle = _getIterationTitleMethod == null
                    ? LoopIteration.ToString()
                    : _getIterationTitleMethod(item);
            }
            catch (Exception)
            {
                // The _getIterationTitleMethod failed, use the current iteration number
                Instance.IterationTitle = LoopIteration.ToString();
            }
#pragma warning disable CS0618 // Type or member is obsolete
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => _methodAsync(item, this, ct), $"Item{LoopIteration}", cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
            LoopIteration++;
            Instance.Iteration = LoopIteration;
            index = LoopIteration - 1;
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
    LoopActivity<IList<TMethodReturns>>, IActivityForEachSequential<TMethodReturns, TItem>
{
    private ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> _methodAsync;
    private readonly GetIterationTitleMethod<TItem> _getIterationTitleMethod;
    public TItem[] Items { get; }

    [JsonIgnore]
    public GetIterationTitleMethod<TItem> GetIterationTitleMethod { get; }

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityForEachSequential(
        IActivityInformation activityInformation, IEnumerable<TItem> items,
        GetIterationTitleMethod<TItem> getIterationTitleMethod = null)
        : base(activityInformation, _ => Task.FromResult((IList<TMethodReturns>)new List<TMethodReturns>()))
    {
        InternalContract.RequireNotNull(items, nameof(items));
        Items = items.ToArray();
        GetIterationTitleMethod = getIterationTitleMethod;
    }

    public ActivityForEachSequential(
        IActivityInformation activityInformation,
        IEnumerable<TItem> items,
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync
        , GetIterationTitleMethod<TItem> getIterationTitleMethod)
        : base(activityInformation, _ => Task.FromResult((IList<TMethodReturns>)new List<TMethodReturns>()))
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
        _getIterationTitleMethod = getIterationTitleMethod;
        Items = items.ToArray();
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public async Task<IList<TMethodReturns>> ExecuteAsync(
        ActivityForEachSequentialMethodAsync<TMethodReturns, TItem> methodAsync,
        CancellationToken cancellationToken = default)
    {
        ChildCounter = 0;
        _methodAsync = methodAsync;
        WorkflowStatic.Context.ParentActivity = this;
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync(ForEachSequentialAsync, DefaultValueMethodAsync, cancellationToken)
            .CatchExitExceptionAsync(this, cancellationToken);
        WorkflowStatic.Context.ParentActivity = null;
        return result;
    }

    internal async Task<IList<TMethodReturns>> ForEachSequentialAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        var resultList = new List<TMethodReturns>();
        if (LoopIteration == 0) LoopIteration = 1;
        var index = LoopIteration - 1;
        while (index < Items.Length) {
            var item = Items[index];
            try
            {
                Instance.IterationTitle = _getIterationTitleMethod == null
                    ? LoopIteration.ToString()
                    : _getIterationTitleMethod(item);
            }
            catch (Exception)
            {
                // The _getIterationTitleMethod failed, use the current iteration number
                Instance.IterationTitle = LoopIteration.ToString();
            }
            var result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => _methodAsync(item, this, ct),
                $"Item{LoopIteration}", cancellationToken);
            resultList.Add(result);
            LoopIteration++;
            Instance.Iteration = LoopIteration;
            index = LoopIteration - 1;
        }

        return resultList;
    }

    /// <inheritdoc />
    protected override async Task<IList<TMethodReturns>> InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync(ForEachSequentialAsync, DefaultValueMethodAsync, cancellationToken);
        return result;
    }
}