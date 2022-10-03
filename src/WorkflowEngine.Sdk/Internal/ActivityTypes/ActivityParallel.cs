using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityParallel" />
internal class ActivityParallel : Activity<JobResults>, IActivityParallel
{
    private readonly Dictionary<int, Task<object>> _objectTasks = new();
    private readonly Dictionary<int, Task> _voidTasks = new();
    private readonly Dictionary<int, object> _objectJobs = new();
    private readonly Dictionary<int, Type> _objectTypes = new();
    private readonly Dictionary<int, ActivityMethodAsync<IActivityParallel>> _voidJobs = new();
    private int _maxJobIndex;

    public ActivityParallel(IActivityInformation activityInformation)
        : base(activityInformation, _ => Task.FromResult(new JobResults()))
    {
        _maxJobIndex = 0;
    }

    /// <inheritdoc />
    public IActivityParallel AddJob(int index, ActivityMethodAsync<IActivityParallel> jobAsync, string jobTitle = null)
    {
        InternalContract.RequireGreaterThan(0, index, nameof(index));
        InternalContract.RequireNotNull(jobAsync, nameof(jobAsync));
        InternalContract.Require(!_voidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        InternalContract.Require(!_objectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        Instance.IterationTitle = jobTitle;
        _voidJobs.Add(index, jobAsync);
        if (index > _maxJobIndex) _maxJobIndex = index;
        return this;
    }

    /// <inheritdoc />
    public IActivityParallel AddJob<TMethodReturns>(int index, ActivityMethodAsync<IActivityParallel, TMethodReturns> jobAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync = null, string jobTitle = null)
    {
        InternalContract.RequireGreaterThan(0, index, nameof(index));
        InternalContract.RequireNotNull(jobAsync, nameof(jobAsync));
        InternalContract.Require(!_voidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        InternalContract.Require(!_objectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        Instance.IterationTitle = jobTitle;
        // Saving different method signatures together is complicated. Step 1: Save them as object.
        _objectJobs.Add(index, jobAsync);
        _objectTypes.Add(index, typeof(TMethodReturns));
        if (index > _maxJobIndex) _maxJobIndex = index;
        return this;
    }

    /// <inheritdoc />
    public int JobNumber
    {
        get => InternalIteration ?? 0;
        private set => InternalIteration = value;
    }

    /// <inheritdoc />
    public async Task<IJobResults> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        JobNumber = 0;
        WorkflowStatic.Context.ParentActivity = this;
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync(ParallelAsync, DefaultValueMethodAsync, cancellationToken);
        WorkflowStatic.Context.ParentActivity = null;
        return result;
    }

    internal async Task<JobResults> ParallelAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (index, job) in _voidJobs)
        {
            JobNumber = index;
            var task = ExecuteJobWithoutReturnValueAsync(job, cancellationToken);
            _voidTasks.Add(index, task);
        }
        foreach (var (index, job) in _objectJobs)
        {
            JobNumber = index;
            var task = ExecuteJobWithReturnValueAsync(job, cancellationToken);
            _objectTasks.Add(index, task);
        }
        var taskList = new List<Task>(_voidTasks.Values);
        taskList.AddRange(_objectTasks.Values);
        await WorkflowHelper.WhenAllActivities(taskList);
        var jobResults = new JobResults();
        foreach (var (index, task) in _objectTasks)
        {
            try
            {
                var o = await task;
                jobResults.Add(index, o, _objectTypes[index]);
            }
            catch (WorkflowImplementationShouldNotCatchThisException outerException)
            {
#pragma warning disable CS0618
                if (outerException.InnerException is not IgnoreAndExitToParentException innerException) throw;
#pragma warning restore CS0618
                FulcrumAssert.IsNotNull(innerException.ActivityFailedException, CodeLocation.AsString());
                var e = innerException.ActivityFailedException;
                await this.LogInformationAsync($"Ignoring exception for parallel job {index}", e, cancellationToken);
            }
        }
        return jobResults;
    }

    private Task ExecuteJobWithoutReturnValueAsync(ActivityMethodAsync<IActivityParallel> job, CancellationToken cancellationToken)
    {
        var task = LogicExecutor.ExecuteWithoutReturnValueAsync(ct => job(this, ct), $"Job{JobNumber}", cancellationToken);
        return task;
    }

    private async Task<object> ExecuteJobWithReturnValueAsync(object job, CancellationToken cancellationToken)
    {
        var result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => HairyWayToCallAsyncMethodWithUnknownReturnType(job, ct), $"Job{JobNumber}", cancellationToken);
        return result;
    }

    /// <summary>
    /// https://stackoverflow.com/questions/48033760/cast-taskt-to-taskobject-in-c-sharp-without-having-t
    /// Step one is in one of the <see cref="AddJob"/> overloads.
    /// </summary>
    private async Task<object> HairyWayToCallAsyncMethodWithUnknownReturnType(object job, CancellationToken cancellationToken)
    {
        // Saving different method signatures together is complicated. Step 2: Convert them to dynamic.
        var convertedJobAsync = (dynamic)job;
        // Saving different method signatures together is complicated. Step 3: Cast the result to Task, no matter what Task<T> they were previously.
        var task = (Task)convertedJobAsync(this, cancellationToken);
        await task;
        // Saving different method signatures together is complicated. Step 4: Cast the task to dynamic, get the Result, cast it to object. Done! Phu!
        var result = (object)((dynamic)task).Result;
        return result;
    }
}