using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Parallel"/>.
/// </summary>
public interface IActivityParallel : IExecutableActivity<IJobResults>
{
    /// <summary>
    /// The number of an individual job in the parallel activity.
    /// </summary>
    int JobNumber { get; }

    /// <summary>
    /// Add a <paramref name="jobAsync"/> to be executed in parallel with all other added jobs.
    /// The <paramref name="index"/> should be > 0 and unique for this job within the parallel activity. It can be used to find the result using <see cref="IJobResults.Get{IActivityParallel}"/>
    /// </summary>
    IActivityParallel AddJob(int index, ActivityMethodAsync<IActivityParallel> jobAsync, string jobTitle = null);

    /// <summary>
    /// Add a <paramref name="jobAsync"/> to be executed in parallel with all other added jobs.
    /// The <paramref name="index"/> should be > 0 and unique for this job within the parallel activity. It can be used to find the result using <see cref="IJobResults.Get{IActivityParallel}"/>
    /// </summary>
    IActivityParallel AddJob<TMethodReturns>(int index, ActivityMethodAsync<IActivityParallel, TMethodReturns> jobAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync = null, string jobTitle = null);
}