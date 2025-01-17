﻿using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivityIf" />
internal class ActivityIf : Activity, IActivityIf, IActivityIfElse
{
    private ActivityMethodAsync<IActivityIf> _thenMethodAsync;
    private ActivityMethodAsync<IActivityIf> _elseMethodAsync;

    public ActivityIf(IActivityInformation activityInformation, ActivityConditionMethodAsync<IActivityIf> conditionMethodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        ConditionMethodAsync = conditionMethodAsync;
    }

    /// <inheritdoc />
    [JsonIgnore]
    public ActivityConditionMethodAsync<IActivityIf> ConditionMethodAsync { get; }

    /// <inheritdoc />
    public IActivityIfElse Then(ActivityMethodAsync<IActivityIf> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityIfElse Then(ActivityMethod<IActivityIf> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = (a, _) =>
        {
            method(a);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity Else(ActivityMethodAsync<IActivityIf> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.Require(_elseMethodAsync == null, "This method can only be called once.");
        _elseMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity Else(ActivityMethod<IActivityIf> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        InternalContract.Require(_elseMethodAsync == null, "This method can only be called once.");
        _elseMethodAsync = (a, _) =>
        {
            method(a);
            return Task.CompletedTask;
        };
        return this;
    }

    internal async Task IfThenElseAsync(CancellationToken cancellationToken = default)
    {
        var condition = await LogicExecutor.ExecuteWithReturnValueAsync(ct => ConditionMethodAsync(this, ct), "If", cancellationToken);
        if (condition)
        {
            if (_thenMethodAsync == null) return;
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => _thenMethodAsync(this, ct), "Then", cancellationToken);
        }
        else
        {
            if (_elseMethodAsync == null) return; 
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => _elseMethodAsync(this, ct), "Else", cancellationToken);
        }
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(IfThenElseAsync, cancellationToken);
    }
}

internal class ActivityIf<TActivityReturns> : Activity<TActivityReturns>, IActivityIf<TActivityReturns>, IActivityIfElse<TActivityReturns>
{
    private ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> _thenMethodAsync;
    private ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> _elseMethodAsync;

    public ActivityIf(
        IActivityInformation activityInformation,
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityConditionMethodAsync<IActivityIf<TActivityReturns>> conditionMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        ConditionMethodAsync = conditionMethodAsync;
    }

    /// <inheritdoc />
    [JsonIgnore]
    public ActivityConditionMethodAsync<IActivityIf<TActivityReturns>> ConditionMethodAsync { get; }

    /// <inheritdoc />
    public IActivityIfElse<TActivityReturns> Then(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityIfElse<TActivityReturns> Then(ActivityMethod<IActivityIf<TActivityReturns>, TActivityReturns> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = (a, _) => Task.FromResult(method(a));
        return this;
    }

    /// <inheritdoc />
    public IActivityIfElse<TActivityReturns> Then(TActivityReturns value)
    {
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = (_, _) => Task.FromResult(value);
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> Else(ActivityMethodAsync<IActivityIf<TActivityReturns>, TActivityReturns> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.Require(_elseMethodAsync == null, "This method can only be called once.");
        _elseMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> Else(ActivityMethod<IActivityIf<TActivityReturns>, TActivityReturns> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        InternalContract.Require(_elseMethodAsync == null, "This method can only be called once.");
        _elseMethodAsync = (a, _) => Task.FromResult(method(a));
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> Else(TActivityReturns value)
    {
        _elseMethodAsync = (_, _) => Task.FromResult(value);
        return this;
    }

    internal async Task<TActivityReturns> IfThenElseAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_thenMethodAsync != null, "An if activity that returns a result must have a then-method.");
        InternalContract.Require(_elseMethodAsync != null, "An if activity that returns a result must have an else-method.");

        var condition = await LogicExecutor.ExecuteWithReturnValueAsync(ct => ConditionMethodAsync(this, ct), "If", cancellationToken);
        TActivityReturns result;
        if (condition)
        {
            result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => _thenMethodAsync(this, ct), "Then", cancellationToken);
        }
        else
        {
            result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => _elseMethodAsync(this, ct), "Else", cancellationToken);
        }

        return result;
    }

    /// <inheritdoc />
    public override Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_thenMethodAsync != null, "An if activity that returns a result must have a then-method.");
        InternalContract.Require(_elseMethodAsync != null, "An if activity that returns a result must have an else-method.");
        return ActivityExecutor.ExecuteWithReturnValueAsync(IfThenElseAsync, DefaultValueMethodAsync, cancellationToken);
    }
}