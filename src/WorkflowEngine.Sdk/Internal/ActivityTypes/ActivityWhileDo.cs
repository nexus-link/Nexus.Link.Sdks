using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

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

    public ActivityWhileDo(IActivityInformation activityInformation,
        ActivityConditionMethod<IActivityWhileDo> conditionMethod)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
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

    /// <inheritdoc />
    protected override async Task InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(WhileDoAsync, cancellationToken);
    }

    internal async Task WhileDoAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        do
        {
            LoopIteration++;
#pragma warning disable CS0618
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => _methodAsync(this, ct), $"Do{LoopIteration}", cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);
#pragma warning restore CS0618
        } while (await LogicExecutor.ExecuteWithReturnValueAsync(ct=> _conditionMethodAsync!(this, ct), "While", cancellationToken));
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

    public ActivityWhileDo(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityConditionMethod<IActivityWhileDo<TActivityReturns>> conditionMethod)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
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

    internal async Task<TActivityReturns> WhileDoAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must call the {nameof(Do)} method.");
        TActivityReturns result;
        do
        {
            LoopIteration++;
            result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => _methodAsync(this, ct), $"Do{LoopIteration}", cancellationToken);
        } while (await LogicExecutor.ExecuteWithReturnValueAsync(ct => _conditionMethodAsync!(this, ct), "While", cancellationToken));

        return result;
    }
}