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

/// <inheritdoc cref="IActivityWhileDo" />
internal class ActivityWhileDo : LoopActivity, IActivityWhileDo
{
    private ActivityMethodAsync<IActivityWhileDo> _methodAsync;
    private readonly ActivityConditionMethodAsync<IActivityWhileDo> _conditionMethodAsync;

    public ActivityWhileDo(IActivityInformation activityInformation,
        ActivityConditionMethodAsync<IActivityWhileDo> conditionMethodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
    }

    internal async Task WhileDoAsync(ActivityMethodAsync<IActivityWhileDo> methodAsync, CancellationToken cancellationToken)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        do
        {
            LoopIteration++;
            await methodAsync(this, cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);
        } while (await _conditionMethodAsync!(this, cancellationToken));
    }

    /// <inheritdoc />
    protected override async Task InternalExecuteAsync(CancellationToken cancellationToken = default)
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
internal class ActivityWhileDo<TActivityReturns> : LoopActivity<TActivityReturns>, IActivityWhileDo<TActivityReturns>
{
    private ActivityMethodAsync<IActivityWhileDo<TActivityReturns>, TActivityReturns> _methodAsync;
    private readonly ActivityConditionMethodAsync<IActivityWhileDo<TActivityReturns>> _conditionMethodAsync;

    public ActivityWhileDo(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityConditionMethodAsync<IActivityWhileDo<TActivityReturns>> conditionMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
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

    /// <inheritdoc />
    protected override async Task<TActivityReturns> InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        return await ActivityExecutor.ExecuteWithReturnValueAsync(WhileDoAsync, DefaultValueMethodAsync, cancellationToken);
    }

    internal async Task<TActivityReturns> WhileDoAsync(CancellationToken cancellationToken)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        TActivityReturns result;
        do
        {
            LoopIteration++;
            result = await _methodAsync!(this, cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);
        } while (await _conditionMethodAsync!(this, cancellationToken));

        return result;
    }
}