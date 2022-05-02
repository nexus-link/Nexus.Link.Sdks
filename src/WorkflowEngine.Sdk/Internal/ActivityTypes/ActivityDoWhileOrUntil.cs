using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityDoWhileOrUntil" />
internal class ActivityDoWhileOrUntil : Activity, IActivityDoWhileOrUntil
{
    private readonly ActivityMethodAsync<IActivityDoWhileOrUntil> _methodAsync;
    private ActivityConditionMethodAsync _conditionMethodAsync;
    private bool _isWhileCondition;

    public ActivityDoWhileOrUntil(IActivityInformation activityInformation,
        ActivityMethodAsync<IActivityDoWhileOrUntil> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(DoWhileOrUntilAsync, cancellationToken);
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil Until(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil Until(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil Until(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil While(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil While(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil While(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = true;
        return this;
    }

    internal async Task DoWhileOrUntilAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        Iteration = 0;
        do
        {
            Iteration++;
            await _methodAsync(this, cancellationToken);
            ActivityInformation.Workflow.LatestActivity = this;
        } while (await GetWhileConditionAsync(cancellationToken));
    }

    internal async Task<bool> GetWhileConditionAsync(CancellationToken cancellationToken = default)
    {
        var condition = await _conditionMethodAsync(this, cancellationToken);
        return _isWhileCondition ? condition : !condition;
    }
}

/// <inheritdoc cref="IActivityDoWhileOrUntil" />
internal class ActivityDoWhileOrUntil<TActivityReturns> : Activity<TActivityReturns>, IActivityDoWhileOrUntil<TActivityReturns>
{
    private readonly ActivityMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>, TActivityReturns> _methodAsync;
    private ActivityConditionMethodAsync _conditionMethodAsync;
    private bool _isWhileCondition;

    public ActivityDoWhileOrUntil(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>, TActivityReturns> methodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
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
        return await ActivityExecutor.ExecuteWithReturnValueAsync(DoWhileOrUntilAsync, DefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil<TActivityReturns> Until(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil<TActivityReturns> Until(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods method can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil<TActivityReturns> Until(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil<TActivityReturns> While(ActivityConditionMethodAsync conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil<TActivityReturns> While(ActivityConditionMethod conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IActivityDoWhileOrUntil<TActivityReturns> While(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = true;
        return this;
    }
    internal async Task<TActivityReturns> DoWhileOrUntilAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
        WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
        TActivityReturns result;
        Iteration = 0;
        do
        {
            Iteration++;
            result = await _methodAsync(this, cancellationToken);
            FulcrumAssert.IsNotNull(Instance.Id, CodeLocation.AsString());
            ActivityInformation.Workflow.LatestActivity = this;
        } while (await GetWhileConditionAsync(cancellationToken));

        return result;
    }
}