using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivityIf" />
internal class ActivityIf : Activity, IActivityIf
{
    private ActivityMethodAsync<IActivityIf> _thenMethodAsync;
    private ActivityMethodAsync<IActivityIf> _elseMethodAsync;

    public ActivityIf(IActivityInformation activityInformation, ActivityIfConditionMethodAsync conditionMethodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        ConditionMethodAsync = conditionMethodAsync;
    }

    /// <inheritdoc />
    public ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <inheritdoc />
    public IActivityIf Then(ActivityMethodAsync<IActivityIf> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityIf Else(ActivityMethodAsync<IActivityIf> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.Require(_elseMethodAsync == null, "This method can only be called once.");
        _elseMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(IfThenElse, cancellationToken);
    }

    private async Task IfThenElse(CancellationToken cancellationToken)
    {
        var condition = await ConditionMethodAsync(this, cancellationToken);
        if (condition)
        {
            if (_thenMethodAsync == null) return;
            await _thenMethodAsync(this, cancellationToken);
        }
        else
        {
            if (_elseMethodAsync == null) return;
            await _elseMethodAsync(this, cancellationToken);
        }
    }
}

internal class ActivityIf<TActivityReturns> : Activity<TActivityReturns>, IActivityIf<TActivityReturns>
{
    private ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> _thenMethodAsync;
    private ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> _elseMethodAsync;

    public ActivityIf(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync, ActivityIfConditionMethodAsync conditionMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        ConditionMethodAsync = conditionMethodAsync;
    }

    /// <inheritdoc />
    public ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <inheritdoc />
    public IActivityIf<TActivityReturns> Then(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync)
    {
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityIf<TActivityReturns> Then(TActivityReturns value)
    {
        _thenMethodAsync = (_, _) => Task.FromResult(value);
        return this;
    }

    /// <inheritdoc />
    public IActivityIf<TActivityReturns> Else(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync)
    {
        InternalContract.Require(_elseMethodAsync == null, "This method can only be called once.");
        _elseMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityIf<TActivityReturns> Else(TActivityReturns value)
    {
        _elseMethodAsync = (_, _) => Task.FromResult(value);
        return this;
    }

    /// <inheritdoc />
    public Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_thenMethodAsync != null, "An if activity that returns a result must have a then-method.");
        InternalContract.Require(_elseMethodAsync != null, "An if activity that returns a result must have an else-method.");
        return ActivityExecutor.ExecuteWithReturnValueAsync(IfThenElse, DefaultValueMethodAsync, cancellationToken);
    }

    private async Task<TActivityReturns> IfThenElse(CancellationToken cancellationToken)
    {
        InternalContract.Require(_thenMethodAsync != null, "An if activity that returns a result must have a then-method.");
        InternalContract.Require(_elseMethodAsync != null, "An if activity that returns a result must have an else-method.");

        var condition = await ConditionMethodAsync(this, cancellationToken);
        TActivityReturns result;
        if (condition)
        {
            result = await _thenMethodAsync!(this, cancellationToken);
        }
        else
        {
            result = await _elseMethodAsync!(this, cancellationToken);
        }

        return result;
    }
}