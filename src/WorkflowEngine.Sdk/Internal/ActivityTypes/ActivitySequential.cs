using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivitySequential" />
internal class ActivitySequential : ActivityJobs<IActivitySequential>, IActivitySequential
{

    public ActivitySequential(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    protected internal override async Task<JobResults> ExecuteJobsAsync(CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        var jobResults = new JobResults();
        for (var i = 0; i < MaxJobIndex; i++)
        {
            Iteration = i;
            ActivityInformation.Workflow.LatestActivity = this;
            if (VoidJobs.ContainsKey(i))
            {
                var job = VoidJobs[i];
                await job(this, cancellationToken);
            } else if (ObjectJobs.ContainsKey(i))
            {
                var job = ObjectJobs[i];
                var task = ExecuteJobAsync(job, cancellationToken);
                var result = GetResult(task);
                jobResults.Add(i, result, ObjectTypes[i]);
            }
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