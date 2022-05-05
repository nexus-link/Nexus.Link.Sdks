using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityForEachParallel{TItem}" />
internal class ActivityForEachParallel<TItem> : LoopActivity, IActivityForEachParallel<TItem>
{
    private readonly ActivityForEachParallelMethodAsync<TItem> _methodAsync;
    public IEnumerable<TItem> Items { get; }

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityForEachParallel(
        IActivityInformation activityInformation,
        IEnumerable<TItem> items)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        Items = items;
    }
    public ActivityForEachParallel(
        IActivityInformation activityInformation,
        IEnumerable<TItem> items,
        ActivityForEachParallelMethodAsync<TItem> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
        Items = items;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public async Task ExecuteAsync(ActivityForEachParallelMethodAsync<TItem> methodAsync, CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync( ct => ForEachParallelAsync(methodAsync, ct), cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ForEachParallelAsync, cancellationToken);
    }

    internal Task ForEachParallelAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        return ForEachParallelAsync(_methodAsync, cancellationToken);
    }

    private async Task ForEachParallelAsync(ActivityForEachParallelMethodAsync<TItem> methodAsync, CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        var taskList = new List<Task>();
        foreach (var item in Items)
        {
            LoopIteration++;
            var task = methodAsync(item, this, cancellationToken);
            taskList.Add(task);
        }

        await WorkflowHelper.WhenAllActivities(taskList);
    }
}

/// <inheritdoc cref="IActivityForEachParallel{TMethodReturns, TItem}" />
internal class ActivityForEachParallel<TMethodReturns, TItem> :
    LoopActivity<IDictionary<string, TMethodReturns>>, IActivityForEachParallel<TMethodReturns, TItem>
{
    private GetKeyMethod<TItem> _getKeyMethod;
    private readonly ActivityForEachParallelMethodAsync<TMethodReturns, TItem> _methodAsync;

    public IEnumerable<TItem> Items { get; }

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityForEachParallel(IActivityInformation activityInformation,
        IEnumerable<TItem> items,
        GetKeyMethod<TItem> getKeyMethod)
        : base(activityInformation, EmptyDictionaryAsync)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
        Items = items;
        _getKeyMethod = getKeyMethod;
    }

    public ActivityForEachParallel(IActivityInformation activityInformation,
        IEnumerable<TItem> items,
        GetKeyMethod<TItem> getKeyMethod,
        ActivityForEachParallelMethodAsync<TMethodReturns, TItem> methodAsync)
        : base(activityInformation, EmptyDictionaryAsync)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
        _methodAsync = methodAsync;
        Items = items;
        _getKeyMethod = getKeyMethod;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task<IDictionary<string, TMethodReturns>> ExecuteAsync(ActivityForEachParallelMethodAsync<TMethodReturns, TItem> methodAsync, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(ct => ForEachParallelAsync(methodAsync, ct), EmptyDictionaryAsync, cancellationToken);
    }

    private static async Task<IDictionary<string, TMethodReturns>> AggregatePostponeExceptions(IDictionary<string, Task<TMethodReturns>> taskDictionary)
    {
        await WorkflowHelper.WhenAllActivities(taskDictionary.Values);
        var resultDictionary = new Dictionary<string, TMethodReturns>();
        foreach (var (key, task) in taskDictionary)
        {
            var result = await task;
            resultDictionary.Add(key, result);
        }
        return resultDictionary;
    }

    /// <inheritdoc />
    protected override async Task<IDictionary<string, TMethodReturns>> InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.ForEachParallel)}() method that has a method as parameter.");
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ForEachParallelAsync, DefaultValueMethodAsync, cancellationToken);
    }

    internal async Task<IDictionary<string, TMethodReturns>> ForEachParallelAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        return await ForEachParallelAsync(_methodAsync, cancellationToken);
    }

    private Task<IDictionary<string, TMethodReturns>> ForEachParallelAsync(
        ActivityForEachParallelMethodAsync<TMethodReturns, TItem> method,
        CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        var taskDictionary = new Dictionary<string, Task<TMethodReturns>>();
        foreach (var item in Items)
        {
            LoopIteration++;
            string key = default;
            try
            {
                key = _getKeyMethod!(item);
            }
            catch (Exception)
            {
                InternalContract.Require(false, $"The {nameof(_getKeyMethod)} method failed. You must make it safe, so that it never fails.");
            }
            InternalContract.Require(key != null, $"The {nameof(_getKeyMethod)} method must not return null.");
            var task = method(item, this, cancellationToken);
            taskDictionary.Add(key!, task);
        }
        return AggregatePostponeExceptions(taskDictionary);
    }

    private static Task<IDictionary<string, TMethodReturns>> EmptyDictionaryAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((IDictionary<string, TMethodReturns>) new Dictionary<string, TMethodReturns>());
    }
}