using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityDoWhileOrUntil" />
internal class ActivityDoWhileOrUntil : LoopActivity, IActivityDoWhileOrUntil
{
    private readonly ActivityMethodAsync<IActivityDoWhileOrUntil> _methodAsync;
    private ActivityConditionMethodAsync<IActivityDoWhileOrUntil> _conditionMethodAsync;
    private bool _isWhileCondition;

    public ActivityDoWhileOrUntil(IActivityInformation activityInformation,
        ActivityMethodAsync<IActivityDoWhileOrUntil> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    /// <inheritdoc />
    protected override async Task InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(DoWhileOrUntilAsync, cancellationToken);
    }

    /// <inheritdoc />
    public IExecutableActivity Until(ActivityConditionMethodAsync<IActivityDoWhileOrUntil> conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity Until(ActivityConditionMethod<IActivityDoWhileOrUntil> conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity Until(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity While(ActivityConditionMethodAsync<IActivityDoWhileOrUntil> conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity While(ActivityConditionMethod<IActivityDoWhileOrUntil> conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity While(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = true;
        return this;
    }

    internal async Task DoWhileOrUntilAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} or the {nameof(While)} method.");
        do
        {
            LoopIteration++;
#pragma warning disable CS0618 // Type or member is obsolete
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => _methodAsync(this, ct), "Do", cancellationToken)
                .CatchExitExceptionAsync(this, cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete
        } while (await GetWhileConditionAsync(cancellationToken));
    }

    internal async Task<bool> GetWhileConditionAsync(CancellationToken cancellationToken = default)
    {
        if (_isWhileCondition)
        {
            return await LogicExecutor.ExecuteWithReturnValueAsync(ct => _conditionMethodAsync(this, ct), "While", cancellationToken);
        }
        return await LogicExecutor.ExecuteWithReturnValueAsync(async ct => !await _conditionMethodAsync(this, ct), "Until", cancellationToken);
    }
}

/// <inheritdoc cref="IActivityDoWhileOrUntil" />
internal class ActivityDoWhileOrUntil<TActivityReturns> : LoopActivity<TActivityReturns>, IActivityDoWhileOrUntil<TActivityReturns>
{
    private readonly ActivityMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>, TActivityReturns> _methodAsync;
    private ActivityConditionMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>> _conditionMethodAsync;
    private bool _isWhileCondition;

    public ActivityDoWhileOrUntil(IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>, TActivityReturns> methodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    /// <inheritdoc />
    protected override async Task<TActivityReturns> InternalExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        return await ActivityExecutor.ExecuteWithReturnValueAsync(DoWhileOrUntilAsync, DefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> Until(ActivityConditionMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> Until(ActivityConditionMethod<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods method can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> Until(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = false;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> While(ActivityConditionMethodAsync<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethodAsync)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        _conditionMethodAsync = conditionMethodAsync;
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> While(ActivityConditionMethod<IActivityDoWhileOrUntil<TActivityReturns>> conditionMethod)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        InternalContract.RequireNotNull(conditionMethod, nameof(conditionMethod));
        _conditionMethodAsync = (a, _) => Task.FromResult(conditionMethod(a));
        _isWhileCondition = true;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> While(bool condition)
    {
        InternalContract.Require(_conditionMethodAsync == null, "The While and Until methods can only be called once.");
        _conditionMethodAsync = (_, _) => Task.FromResult(condition);
        _isWhileCondition = true;
        return this;
    }
    internal async Task<TActivityReturns> DoWhileOrUntilAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_conditionMethodAsync != null, $"You must call the {nameof(Until)} method.");
        TActivityReturns result;
        do
        {
            LoopIteration++;
            result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => _methodAsync(this, ct), "Do", cancellationToken);
        } while (await GetWhileConditionAsync(cancellationToken));

        return result;
    }

    internal async Task<bool> GetWhileConditionAsync(CancellationToken cancellationToken = default)
    {
        if (_isWhileCondition)
        {
            return await LogicExecutor.ExecuteWithReturnValueAsync(ct => _conditionMethodAsync(this, ct), "While", cancellationToken);
        }
        return await LogicExecutor.ExecuteWithReturnValueAsync(async ct => !await _conditionMethodAsync(this, ct), "Until", cancellationToken);
    }
}