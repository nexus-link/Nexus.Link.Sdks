using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// The implementation method for a parallel activity with no return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction"/>.</param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task ActivityParallelMethodAsync(IActivityParallel activity, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for a parallel activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction"/>.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TMethodReturns">The type of the returned value from the method</typeparam>
public delegate Task<TMethodReturns> ActivityParallelMethodAsync<TMethodReturns>(IActivityParallel activity, CancellationToken cancellationToken);

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Parallel"/>.
/// </summary>
public interface IActivityParallel : IActivity
{
    /// <summary>
    /// Add a <paramref name="jobAsync"/> to be executed in parallel with all other added jobs.
    /// The <paramref name="index"/> should be > 0 and unique for this job within the parallel activity. It can be used to find the result using <see cref="IJobResults.Get{T}"/>
    /// </summary>
    IActivityParallel AddJob(int index, ActivityParallelMethodAsync jobAsync);

    /// <summary>
    /// Add a <paramref name="jobAsync"/> to be executed in parallel with all other added jobs.
    /// The <paramref name="index"/> should be > 0 and unique for this job within the parallel activity. It can be used to find the result using <see cref="IJobResults.Get{T}"/>
    /// </summary>
    IActivityParallel AddJob<TMethodReturns>(int index, ActivityParallelMethodAsync<TMethodReturns> jobAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync = null);

    /// <summary>
    /// Execute all jobs, <see cref="AddJob{TMethodReturns}"/> and <see cref="AddJob"/>.
    /// </summary>
    Task<IJobResults> ExecuteAsync(CancellationToken cancellationToken = default);
}