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
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityForEachParallel{TItem}" />
internal class ActivityForEachParallel<TItem> : Activity, IActivityForEachParallel<TItem>
{
    public IEnumerable<TItem> Items { get; }

    public ActivityForEachParallel(IActivityInformation activityInformation, IEnumerable<TItem> items)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        Items = items;
        Iteration = 0;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(ActivityForEachParallelMethodAsync<TItem> methodAsync, CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync( ct => ForEachMethod(methodAsync, ct), cancellationToken);
    }

    private async Task ForEachMethod(ActivityForEachParallelMethodAsync<TItem> methodAsync, CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        var taskList = new List<Task>();
        foreach (var item in Items)
        {
            Iteration++;
            ActivityInformation.Workflow.LatestActivity = this;
            var task = methodAsync(item, this, cancellationToken);
            taskList.Add(task);
        }
        ActivityInformation.Workflow.LatestActivity = this;

        await WorkflowHelper.WhenAllActivities(taskList);
    }
}

/// <inheritdoc cref="IActivityForEachParallel{TMethodReturns, TItem}" />
internal class ActivityForEachParallel<TMethodReturns, TItem> : Activity, IActivityForEachParallel<TMethodReturns, TItem>
{
    private Func<TItem, string> _getKeyMethod;

    public IEnumerable<TItem> Items { get; }

    public ActivityForEachParallel(IActivityInformation activityInformation, IEnumerable<TItem> items, Func<TItem, string> getKeyMethod)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(items, nameof(items));
        InternalContract.RequireNotNull(getKeyMethod, nameof(getKeyMethod));
        Items = items;
        Iteration = 0;
        _getKeyMethod = getKeyMethod;
    }

    /// <inheritdoc/>
    public Task<IDictionary<string, TMethodReturns>> ExecuteAsync(ActivityForEachParallelMethodAsync<TMethodReturns, TItem> method, CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(
            ct => ForEachMethod(method, ct), 
            _ => Task.FromResult((IDictionary<string, TMethodReturns>)new Dictionary<string, TMethodReturns>()), 
            cancellationToken);
    }

    private Task<IDictionary<string, TMethodReturns>> ForEachMethod(
        ActivityForEachParallelMethodAsync<TMethodReturns, TItem> method,
        CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        var taskDictionary = new Dictionary<string, Task<TMethodReturns>>();
        foreach (var item in Items)
        {
            Iteration++;
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
            ActivityInformation.Workflow.LatestActivity = this;
            var task = method(item, this, cancellationToken);
            taskDictionary.Add(key!, task);
        }
        ActivityInformation.Workflow.LatestActivity = this;
        return AggregatePostponeExceptions(taskDictionary);
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
}