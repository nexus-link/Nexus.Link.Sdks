﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

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
    IActivityParallel AddJob(int index, ActivityMethodAsync<IActivityParallel> jobAsync);

    /// <summary>
    /// Add a <paramref name="jobAsync"/> to be executed in parallel with all other added jobs.
    /// The <paramref name="index"/> should be > 0 and unique for this job within the parallel activity. It can be used to find the result using <see cref="IJobResults.Get{IActivityParallel}"/>
    /// </summary>
    IActivityParallel AddJob<TMethodReturns>(int index, ActivityMethodAsync<IActivityParallel, TMethodReturns> jobAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync = null);
}