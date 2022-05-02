using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityDoUntil" />
internal class ActivityDoUntil : Activity, IActivityDoUntil
{
    private readonly ActivityMethodAsync<IActivityDoUntil> _methodAsync;
    private ActivityConditionMethodAsync _conditionMethodAsync;
    private bool _isWhileCondition;

    public ActivityDoUntil(IActivityInformation activityInformation,
        ActivityMethodAsync<IActivityDoUntil> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    internal async Task DoUntilAsync(ActivityMethodAsync<IActivityDoUntil> methodAsync, CancellationToken cancellationToken)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        do
        {
            Iteration++;
            await methodAsync(this, cancellationToken);
            ActivityInformation.Workflow.LatestActivity = this;
        } while (await GetWhileConditionAsync(cancellationToken));
    }

    internal async Task<bool> GetWhileConditionAsync(CancellationToken cancellationToken)
    {
        var condition = await _conditionMethodAsync(this, cancellationToken);
        return _isWhileCondition ? condition : !condition;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => DoUntilAsync(_methodAsync, ct), cancellationToken);
    }

    /// <inheritdoc />
    public IActivityDoUntil Until(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil Until(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil Until(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil While(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil While(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil While(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = true;
        return this;
    }
}

/// <inheritdoc cref="IActivityDoUntil{TActivityReturns}" />
internal class ActivityDoUntil<TActivityReturns> : Activity<TActivityReturns>, IActivityDoUntil<TActivityReturns>
{
    private readonly ActivityMethodAsync<IActivityDoUntil<TActivityReturns>, TActivityReturns> _methodAsync;
    private ActivityConditionMethodAsync _conditionMethodAsync;
    private bool _isWhileCondition;

    public ActivityDoUntil(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityMethodAsync<IActivityDoUntil<TActivityReturns>, TActivityReturns> methodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        _methodAsync = methodAsync;
    }

    internal async Task<TActivityReturns> DoUntilAsync(ActivityMethodAsync<IActivityDoUntil<TActivityReturns>, TActivityReturns> method, CancellationToken cancellationToken)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        TActivityReturns result;
        do
        {
            Iteration++;
            result = await method(this, cancellationToken);
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            ActivityInformation.Workflow.LatestActivity = this;
        } while (await GetWhileConditionAsync(cancellationToken));

        return result;
    }

    internal async Task<bool> GetWhileConditionAsync(CancellationToken cancellationToken)
    {
        var condition = await _conditionMethodAsync(this, cancellationToken);
        return _isWhileCondition ? condition : !condition;
    }

    /// <inheritdoc />
    public async Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ct => DoUntilAsync(_methodAsync, ct), DefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc />
    public IActivityDoUntil<TActivityReturns> Until(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil<TActivityReturns> Until(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods method can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil<TActivityReturns> Until(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil<TActivityReturns> While(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil<TActivityReturns> While(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoUntil<TActivityReturns> While(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = true;
        return this;
    }
}