using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <inheritdoc cref="IParallelActivity" />
internal abstract class ParallelActivity : Activity, IParallelActivity, IExecutableActivity
{
    protected ParallelActivity(IActivityInformation activityInformation)
        : base(activityInformation)
    {
        JobNumber = 0;
    }

    /// <inheritdoc />
    public int JobNumber
    {
        get => InternalIteration ?? 0;
        protected set => InternalIteration = value;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivity = this;
        await InternalExecuteAsync(cancellationToken);
    }

    protected abstract Task InternalExecuteAsync(CancellationToken cancellationToken);
}

/// <inheritdoc cref="IParallelActivity" />
internal abstract class ParallelActivity<TActivityReturns> : Activity<TActivityReturns>, IParallelActivity, IExecutableActivity<TActivityReturns>
{

    protected ParallelActivity(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        JobNumber = 0;
    }

    /// <inheritdoc />
    public int JobNumber
    {
        get => InternalIteration ?? 0;
        protected set => InternalIteration = value;
    }

    /// <inheritdoc />
    public async Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivity = this;
        return await InternalExecuteAsync(cancellationToken);
    }

    protected abstract Task<TActivityReturns> InternalExecuteAsync(CancellationToken cancellationToken);
}