using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityLoopUntilTrueBase" />
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
internal abstract class ActivityLoopUntilTrueBase : Activity, IActivityLoopUntilTrueBase
{
    private readonly Dictionary<string, object> _loopArguments = new();

    /// <inheritdoc/>
    public bool? EndLoop { get; set; }

    protected ActivityLoopUntilTrueBase(IActivityInformation activityInformation)
        : base(activityInformation)
    {
        Iteration = 0;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the GetContext() method. Obsolete since 2022-05-01.")]
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
}

/// <inheritdoc cref="IActivityLoopUntilTrue" />
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
internal class ActivityLoopUntilTrue : ActivityLoopUntilTrueBase, IActivityLoopUntilTrue
{
    private readonly ActivityMethodAsync<IActivityLoopUntilTrue> _methodAsync;

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityLoopUntilTrue(IActivityInformation activityInformation)
        : base(activityInformation)
    {
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
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => LoopUntilAsync(methodAsync, ct), cancellationToken);
    }

    internal async Task LoopUntilAsync(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync, CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        EndLoop = null;
        do
        {
            Iteration++;
            // TODO: Verify that we don't use the same values each iteration
            await methodAsync(this, cancellationToken);
            InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            ActivityInformation.Workflow.LatestActivity = this;
        } while (EndLoop != true);
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.LoopUntil)}() method that has a method as parameter.");
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => LoopUntilAsync(_methodAsync, ct), cancellationToken);
    }
}

/// <inheritdoc cref="IActivityLoopUntilTrue{TActivityReturns}" />
[Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
internal class ActivityLoopUntilTrue<TActivityReturns> : ActivityLoopUntilTrueBase, IActivityLoopUntilTrue<TActivityReturns>
{
    private readonly ActivityDefaultValueMethodAsync<TActivityReturns> _getDefaultValueAsync;
    private readonly ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> _methodAsync;

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityLoopUntilTrue(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync)
        : base(activityInformation)
    {
        _getDefaultValueAsync = getDefaultValueAsync;
    }

    [Obsolete("Please use DoUntil or WhileDo. Obsolete since 2022-05-02.")]
    public ActivityLoopUntilTrue(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync,
        ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync)
        : base(activityInformation)
    {
        _getDefaultValueAsync = getDefaultValueAsync;
        _methodAsync = methodAsync;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method parameter in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public async Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ct => LoopUntilAsync(methodAsync, ct),
            _getDefaultValueAsync, cancellationToken);
    }

    internal async Task<TActivityReturns> LoopUntilAsync(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> method, CancellationToken cancellationToken)
    {
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        EndLoop = null;
        TActivityReturns result;
        do
        {
            Iteration++;
            // TODO: Verify that we don't use the same values each iteration
            result = await method(this, cancellationToken);
            InternalContract.RequireNotNull(EndLoop, "ignore", $"You must set {nameof(EndLoop)} before returning.");
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            ActivityInformation.Workflow.LatestActivity = this;
        } while (EndLoop != true);

        return result;
    }

    /// <inheritdoc />
    public async Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ct => LoopUntilAsync(_methodAsync, ct), _getDefaultValueAsync, cancellationToken);
    }
}