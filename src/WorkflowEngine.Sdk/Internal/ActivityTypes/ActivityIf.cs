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
    private ActivityIfMethodAsync _thenMethodAsync;
    private ActivityIfMethodAsync _elseMethodAsync;

    public ActivityIf(IActivityInformation activityInformation, ActivityIfConditionMethodAsync conditionMethodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        ConditionMethodAsync = conditionMethodAsync;
    }

    /// <inheritdoc />
    public ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <inheritdoc />
    public IActivityIf Then(ActivityIfMethodAsync methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityIf Else(ActivityIfMethodAsync methodAsync)
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
    private ActivityIfMethodAsync<TActivityReturns> _thenMethodAsync;
    private ActivityIfMethodAsync<TActivityReturns> _elseMethodAsync;

    public ActivityIf(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueMethodAsync, ActivityIfConditionMethodAsync conditionMethodAsync)
        : base(activityInformation, getDefaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(conditionMethodAsync, nameof(conditionMethodAsync));
        ConditionMethodAsync = conditionMethodAsync;
    }

    /// <inheritdoc />
    public ActivityIfConditionMethodAsync ConditionMethodAsync { get; }

    /// <inheritdoc />
    public IActivityIf<TActivityReturns> Then(ActivityIfMethodAsync<TActivityReturns> methodAsync)
    {
        InternalContract.Require(_thenMethodAsync == null, "This method can only be called once.");
        _thenMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivityIf<TActivityReturns> Else(ActivityIfMethodAsync<TActivityReturns> methodAsync)
    {
        InternalContract.Require(_elseMethodAsync == null, "This method can only be called once.");
        _elseMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_thenMethodAsync != null, "An if activity that returns a result must have a then-method.");
        InternalContract.Require(_elseMethodAsync != null, "An if activity that returns a result must have an else-method.");
        return ActivityExecutor.ExecuteWithReturnValueAsync(IfThenElse, GetDefaultValueMethodAsync, cancellationToken);
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