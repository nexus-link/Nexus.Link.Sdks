using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityParallel" />
internal class ActivityParallel : ActivityJobs<IActivityParallel>, IActivityParallel
{
    private readonly Dictionary<int, Task> _objectTasks = new();
    private readonly Dictionary<int, Task> _voidTasks = new();

    public ActivityParallel(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    protected internal override async Task<JobResults> ExecuteJobsAsync(CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        foreach (var (index, job) in VoidJobs)
        {
            ChildCounter = index;
            ActivityInformation.Workflow.LatestActivity = this;
            var task = job(this, cancellationToken);
            _voidTasks.Add(index, task);
        }
        foreach (var (index, job) in ObjectJobs)
        {
            ChildCounter = index;
            ActivityInformation.Workflow.LatestActivity = this;
            var task = ExecuteJobAsync(job, cancellationToken);
            _objectTasks.Add(index, task);
        }
        ActivityInformation.Workflow.LatestActivity = this;
        var taskList = new List<Task>(_voidTasks.Values);
        taskList.AddRange(_objectTasks.Values);
        await WorkflowHelper.WhenAllActivities(taskList);
        var jobResults = new JobResults();
        foreach (var (index, task) in _objectTasks)
        {
            var result = GetResult(task);
            jobResults.Add(index, result, ObjectTypes[index]);
        }
        return jobResults;
    }

    private Task ExecuteJobAsync(object job, CancellationToken cancellationToken)
    {
        // Saving different method signatures together is complicated. Step 2: Convert them to dynamic.
        var convertedJobAsync = (dynamic)job;
        // Saving different method signatures together is complicated. Step 3: Cast the result to Task, no matter what Task<T> they were previously.
        var task = (Task)convertedJobAsync(this, cancellationToken);
        return task;
    }
}