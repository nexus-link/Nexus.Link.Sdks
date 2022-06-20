using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

/// <inheritdoc cref="ILoopActivity" />
internal abstract class LoopActivity : Activity, ILoopActivity, IExecutableActivity
{
    protected LoopActivity(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(LoopIteration)}. Obsolete since 2022-05-08.")]
    public int ChildCounter
    {
        get => InternalIteration ?? 0; 
        protected set => InternalIteration = value;
    }

    /// <inheritdoc />
    public int LoopIteration
    {
        get => InternalIteration ?? 0;
        protected set => InternalIteration = value;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivity = this;
        await InternalExecuteAsync(cancellationToken);
        WorkflowStatic.Context.ParentActivity = null;
    }

    protected abstract Task InternalExecuteAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="ILoopActivity" />
internal abstract class LoopActivity<TActivityReturns> : Activity<TActivityReturns>, ILoopActivity, IExecutableActivity<TActivityReturns>
{
    protected LoopActivity(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
    }

    /// <inheritdoc />
    [Obsolete($"Please use {nameof(LoopIteration)}. Obsolete since 2022-05-08.")]
    public int ChildCounter
    {
        get => InternalIteration ?? 0;
        protected set => InternalIteration = value;
    }

    /// <inheritdoc />
    public int LoopIteration
    {
        get => InternalIteration ?? 0;
        protected set => InternalIteration = value;
    }

    /// <inheritdoc />
    public async Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        LoopIteration = 0;
        WorkflowStatic.Context.ParentActivity = this;
        var result =await InternalExecuteAsync(cancellationToken);
        WorkflowStatic.Context.ParentActivity = null;
        return result;
    }

    protected abstract Task<TActivityReturns> InternalExecuteAsync(CancellationToken cancellationToken = default);
}