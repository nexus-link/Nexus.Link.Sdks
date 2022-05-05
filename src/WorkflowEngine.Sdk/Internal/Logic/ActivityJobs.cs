using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <inheritdoc cref="IActivityJobs{T}" />
internal abstract class ActivityJobs<T> : ParallelActivity<IJobResults>, IActivityJobs<T>
where T : class, IActivityJobs<T>
{
    protected readonly Dictionary<int, object> ObjectJobs = new();
    protected readonly Dictionary<int, Type> ObjectTypes = new();
    protected readonly Dictionary<int, ActivityMethodAsync<T>> VoidJobs = new();
    protected int MaxJobIndex;

    protected ActivityJobs(IActivityInformation activityInformation)
        : base(activityInformation, null)
    {
        MaxJobIndex = 0;
    }

    /// <inheritdoc />
    public T AddJob(int index, ActivityMethodAsync<T> jobAsync)
    {
        InternalContract.RequireGreaterThan(0, index, nameof(index));
        InternalContract.RequireNotNull(jobAsync, nameof(jobAsync));
        InternalContract.Require(!VoidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        InternalContract.Require(!ObjectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        VoidJobs.Add(index, jobAsync);
        if (index > MaxJobIndex) MaxJobIndex = index;
        var cast = this as T;
        FulcrumAssert.IsNotNull(cast, CodeLocation.AsString());
        return cast;
    }

    /// <inheritdoc />
    public T AddJob<TMethodReturns>(int index, ActivityMethodAsync<T, TMethodReturns> jobAsync, ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync = null)
    {
        InternalContract.RequireGreaterThan(0, index, nameof(index));
        InternalContract.RequireNotNull(jobAsync, nameof(jobAsync));
        InternalContract.Require(!VoidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        InternalContract.Require(!ObjectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
        // Saving different method signatures together is complicated. Step 1: Save them as object.
        ObjectJobs.Add(index, jobAsync);
        ObjectTypes.Add(index, typeof(TMethodReturns));
        if (index > MaxJobIndex) MaxJobIndex = index;
        var cast = this as T;
        FulcrumAssert.IsNotNull(cast, CodeLocation.AsString());
        return cast;
    }

    protected internal abstract Task<JobResults> ExecuteJobsAsync(CancellationToken cancellationToken = default);

    protected static object GetResult(Task task)
    {
        // Saving different method signatures together is complicated. Step 4: Cast the task to dynamic, get the Result, cast it to object. Done! Phu!
        // https://stackoverflow.com/questions/48033760/cast-taskt-to-taskobject-in-c-sharp-without-having-t
        return (object)((dynamic)task).Result;
    }

    /// <inheritdoc />
    protected override async Task<IJobResults> InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync(ExecuteJobsAsync, null, cancellationToken);
        return result;
    }
}