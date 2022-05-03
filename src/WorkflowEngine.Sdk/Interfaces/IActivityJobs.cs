﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// Common base interface for <see cref="IActivitySequential"/> and <see cref="IActivityParallel"/>
/// </summary>
public interface IActivityJobs<out T> : 
    IExecutableActivity<IJobResults>, IParallelActivity
{
    /// <summary>
    /// Add a <paramref name="jobAsync"/> to be executed in parallel with all other added jobs.
    /// The <paramref name="index"/> should be > 0 and unique for this job within the parallel activity. It can be used to find the result using <see cref="IJobResults.Get{T}"/>
    /// </summary>
    T AddJob(int index, ActivityMethodAsync<T> jobAsync);

    /// <summary>
    /// Add a <paramref name="jobAsync"/> to be executed in parallel with all other added jobs.
    /// The <paramref name="index"/> should be > 0 and unique for this job within the parallel activity. It can be used to find the result using <see cref="IJobResults.Get{T}"/>
    /// </summary>
    T AddJob<TMethodReturns>(int index, ActivityMethodAsync<T, TMethodReturns> jobAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync = null);
}