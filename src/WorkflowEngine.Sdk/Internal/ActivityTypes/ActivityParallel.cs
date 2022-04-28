using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

/// <inheritdoc cref="IActivityParallel" />
internal class ActivityParallel : Activity, IActivityParallel
{
    private readonly Dictionary<int, object> _objectJobs = new();
    private readonly Dictionary<int, Type> _objectTypes = new();
    private readonly Dictionary<int, Task> _objectTasks = new();
    private readonly Dictionary<int, ActivityParallelMethodAsync> _voidJobs = new();
    private readonly Dictionary<int, Task> _voidTasks = new();

    public ActivityParallel(IActivityInformation activityInformation)
        : base(activityInformation)
    {
        Iteration = 0;
    }

    /// <inheritdoc />
    public IActivityParallel AddJob(int index, ActivityParallelMethodAsync jobAsync)
    {
        InternalContract.RequireGreaterThan(0, index, nameof(index));
        InternalContract.RequireNotNull(jobAsync, nameof(jobAsync));
        InternalContract.Require(!_voidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        InternalContract.Require(!_objectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        _voidJobs.Add(index, jobAsync);
        return this;
    }

    /// <inheritdoc />
    public IActivityParallel AddJob<TMethodReturns>(int index, ActivityParallelMethodAsync<TMethodReturns> jobAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync = null)
    {
        InternalContract.RequireGreaterThan(0, index, nameof(index));
        InternalContract.RequireNotNull(jobAsync, nameof(jobAsync));
        InternalContract.Require(!_voidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        InternalContract.Require(!_objectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        // Saving different method signatures together is complicated. Step 1: Save them as object.
        _objectJobs.Add(index, jobAsync);
        _objectTypes.Add(index, typeof(TMethodReturns));
        return this;
    }

    /// <inheritdoc />
    public async Task<IJobResults> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync(ExecuteJobsAsync, null, cancellationToken);
        return result;
    }

    internal async Task<JobResults> ExecuteJobsAsync(CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        foreach (var (index, job) in _voidJobs)
        {
            Iteration = index;
            ActivityInformation.Workflow.LatestActivity = this;
            var task = job(this, cancellationToken);
            _voidTasks.Add(index, task);
        }
        foreach (var (index, job) in _objectJobs)
        {
            Iteration = index;
            ActivityInformation.Workflow.LatestActivity = this;
            // Saving different method signatures together is complicated. Step 2: Convert them to dynamic.
            var convertedJobAsync = (dynamic) job;
            // Saving different method signatures together is complicated. Step 3: Cast the result to Task, no matter what Task<T> they were previously.
            var task = (Task)convertedJobAsync(this, cancellationToken);
            _objectTasks.Add(index, task);
        }
        ActivityInformation.Workflow.LatestActivity = this;
        var taskList = new List<Task>(_voidTasks.Values);
        taskList.AddRange(_objectTasks.Values);
        await WorkflowHelper.WhenAllActivities(taskList);
        var jobResults = new JobResults();
        foreach (var (index, task) in _objectTasks)
        {
            // Saving different method signatures together is complicated. Step 4: Cast the task to dynamic, get the Result, cast it to object. Done! Phu!
            // https://stackoverflow.com/questions/48033760/cast-taskt-to-taskobject-in-c-sharp-without-having-t
            var result =  (object)((dynamic)task).Result;
            jobResults.Add(index, result, _objectTypes[index]);
        }
        return jobResults;
    }
}