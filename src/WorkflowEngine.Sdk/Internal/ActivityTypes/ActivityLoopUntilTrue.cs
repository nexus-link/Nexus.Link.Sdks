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
    public void SetLoopArgument<T>(string name, T value)
    {
        _loopArguments[name] = value;
    }

    /// <inheritdoc/>
    public T GetLoopArgument<T>(string name)
    {
        if (!_loopArguments.ContainsKey(name)) return default;
        return (T)_loopArguments[name];
    }
}

/// <inheritdoc cref="IActivityLoopUntilTrue" />
internal class ActivityLoopUntilTrue : ActivityLoopUntilTrueBase, IActivityLoopUntilTrue
{
    public ActivityLoopUntilTrue(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(
        ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync,
        CancellationToken cancellationToken = default)
    {
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => LoopUntilMethod(methodAsync, ct), cancellationToken);
    }

    private async Task LoopUntilMethod(ActivityMethodAsync<IActivityLoopUntilTrue> methodAsync, CancellationToken cancellationToken)
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
}

/// <inheritdoc cref="IActivityLoopUntilTrue{TActivityReturns}" />
internal class ActivityLoopUntilTrue<TActivityReturns> : ActivityLoopUntilTrueBase, IActivityLoopUntilTrue<TActivityReturns>
{
    private readonly ActivityDefaultValueMethodAsync<TActivityReturns> _getDefaultValueAsync;

    public ActivityLoopUntilTrue(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync)
        : base(activityInformation)
    {
        _getDefaultValueAsync = getDefaultValueAsync;
    }

    /// <inheritdoc/>
    public async Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ct => LoopUntilMethod(methodAsync, ct),
            _getDefaultValueAsync, cancellationToken);
    }

    private async Task<TActivityReturns> LoopUntilMethod(ActivityMethodAsync<IActivityLoopUntilTrue<TActivityReturns>, TActivityReturns> method, CancellationToken cancellationToken)
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
}