using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityLoopUntilTrue" />
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
internal class ActivityLoopUntilTrue : LoopActivity, IActivityLoopUntilTrue
{
    private readonly ActivityMethodAsync<IActivityLoopUntilTrue> _methodAsync;
    private readonly Dictionary<string, object> _loopArguments = new();

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityLoopUntilTrue(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    /// <inheritdoc/>
    public bool? EndLoop { get; set; }

    /// <inheritdoc/>
    [Obsolete("Please use the GetInternalContext() method. Obsolete since 2022-05-01.")]
    public T GetLoopArgument<T>(string name)
    {
        if (!_loopArguments.ContainsKey(name)) return default;
        return (T)_loopArguments[name];
    }

    /// <inheritdoc/>
    [Obsolete("Please use the SetContext() method. Obsolete since 2022-05-01.")]
    public void SetLoopArgument<T>(string name, T value)
    {
        _loopArguments[name] = value;
    }

    [Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
    public ActivityLoopUntilTrue(IActivityInformation activityInformation,
        ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public async Task ExecuteAsync(
        ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync,
        CancellationToken cancellationToken = default)
    {
        WorkflowStatic.Context.ParentActivity = this;
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => LoopUntilAsync(methodAsync, ct), cancellationToken);
    }

    internal async Task LoopUntilAsync(CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        await LoopUntilAsync(_methodAsync, cancellationToken);
    }

    private async Task LoopUntilAsync(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync, CancellationToken cancellationToken)
    {
        EndLoop = null;
        do
        {
            LoopIteration++;
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => methodAsync(this, ct), "Loop", cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);
            InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
        } while (EndLoop != true);
    }

    /// <inheritdoc />
    protected override async Task InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.LoopUntil)}() method that has a method as parameter.");
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(LoopUntilAsync, cancellationToken);
    }
}

/// <inheritdoc cref="IActivityLoopUntilTrue{TActivityReturns}" />
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
internal class ActivityLoopUntilTrue<TActivityReturns> : LoopActivity<TActivityReturns>, IActivityLoopUntilTrue<TActivityReturns>
{
    private readonly ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> _methodAsync;

    private readonly Dictionary<string, object> _loopArguments = new();

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityLoopUntilTrue(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync)
        : base(activityInformation, getDefaultValueAsync)
    {
    }

    [Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
    public ActivityLoopUntilTrue(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync,
        ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync)
        : base(activityInformation, getDefaultValueAsync)
    {
        _methodAsync = methodAsync;
    }

    /// <inheritdoc/>
    public bool? EndLoop { get; set; }

    /// <inheritdoc/>
    [Obsolete("Please use the GetInternalContext() method. Obsolete since 2022-05-01.")]
    public T GetLoopArgument<T>(string name)
    {
        if (!_loopArguments.ContainsKey(name)) return default;
        return (T)_loopArguments[name];
    }

    /// <inheritdoc/>
    [Obsolete("Please use the SetContext() method. Obsolete since 2022-05-01.")]
    public void SetLoopArgument<T>(string name, T value)
    {
        _loopArguments[name] = value;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method parameter in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public async Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        WorkflowStatic.Context.ParentActivity = this;
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ct => LoopUntilAsync(methodAsync, ct), DefaultValueMethodAsync, cancellationToken);
    }

    internal async Task<TActivityReturns> LoopUntilAsync(CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        return await LoopUntilAsync(_methodAsync, cancellationToken);
    }

    private async Task<TActivityReturns> LoopUntilAsync(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken)
    {
        EndLoop = null;
        TActivityReturns result;
        do
        {
            LoopIteration++;
            // TODO: Verify that we don't use the same values each iteration
            result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => methodAsync(this, ct), "Loop", cancellationToken)
                    .CatchExitExceptionAsync(this, cancellationToken);
            InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
        } while (EndLoop != true);

        return result;
    }

    /// <inheritdoc />
    protected override async Task<TActivityReturns> InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
        return await ActivityExecutor.ExecuteWithReturnValueAsync(LoopUntilAsync, DefaultValueMethodAsync, cancellationToken);
    }
}