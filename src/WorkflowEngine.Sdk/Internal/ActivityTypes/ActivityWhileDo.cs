using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityWhileDo" />
internal class ActivityWhileDo : Activity, IActivityWhileDo
{
    private ActivityMethodAsync<IActivityWhileDo> _methodAsync;
    private readonly ActivityConditionMethodAsync _conditionMethodAsync;

    public ActivityWhileDo(IActivityInformation activityInformation,
        ActivityConditionMethodAsync conditionMethodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
    }

    internal async Task WhileDoAsync(ActivityMethodAsync<IActivityWhileDo> methodAsync, CancellationToken cancellationToken)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        do
        {
            Iteration++;
            await methodAsync(this, cancellationToken);
            ActivityInformation.Workflow.LatestActivity = this;
        } while (await _conditionMethodAsync!(this, cancellationToken));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => WhileDoAsync(_methodAsync, ct), cancellationToken);
    }

    /// <inheritdoc />
    public IActivityWhileDo Do(ActivityMethodAsync<IActivityWhileDo> methodAsync)
    {
        InternalContract.Require(_methodAsync == null, "This method can only be called once.");
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityWhileDo Do(ActivityMethod<IActivityWhileDo> method)
    {
        InternalContract.Require(_methodAsync == null, "This method can only be called once.");
        InternalContract.RequireNotNull(method, nameof(method));
        _methodAsync = (a, _) =>
        {
            method(a);
            return Task.CompletedTask;
        };
        return this;
    }
}

/// <inheritdoc cref="IActivityWhileDo{TActivityReturns}" />
internal class ActivityWhileDo<TActivityReturns> : Activity<TActivityReturns>, IActivityWhileDo<TActivityReturns>
{
    private ActivityMethodAsync<IActivityWhileDo<TActivityReturns>, TActivityReturns> _methodAsync;
    private readonly ActivityConditionMethodAsync _conditionMethodAsync;

    public ActivityWhileDo(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityConditionMethodAsync conditionMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        _conditionMethodAsync = conditionMethodAsync;
    }

    internal async Task<TActivityReturns> WhileDoAsync(ActivityMethodAsync<IActivityWhileDo<TActivityReturns>, TActivityReturns> method, CancellationToken cancellationToken)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        TActivityReturns result;
        do
        {
            Iteration++;
            result = await method(this, cancellationToken);
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            ActivityInformation.Workflow.LatestActivity = this;
        } while (await _conditionMethodAsync!(this, cancellationToken));

        return result;
    }

    /// <inheritdoc />
    public async Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ct => WhileDoAsync(_methodAsync, ct), DefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc />
    public IActivityWhileDo<TActivityReturns> Do(ActivityMethodAsync<IActivityWhileDo<TActivityReturns>, TActivityReturns> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityWhileDo<TActivityReturns> Do(ActivityMethod<IActivityWhileDo<TActivityReturns>, TActivityReturns> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        _methodAsync = (a, _) => Task.FromResult(method(a));
        return this;
    }

    /// <inheritdoc />
    public IActivityWhileDo<TActivityReturns> Do(TActivityReturns condition)
    {
        _methodAsync = (_, _) => Task.FromResult(condition);
        return this;
    }
}