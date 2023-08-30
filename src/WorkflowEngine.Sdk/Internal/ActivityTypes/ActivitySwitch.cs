using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivitySwitch{TSwitchValue}" />
internal class ActivitySwitch<TSwitchValue> : Activity, IActivitySwitch<TSwitchValue>
    where TSwitchValue : IComparable, IComparable<TSwitchValue>
{
    private readonly Dictionary<TSwitchValue, ActivitySwitchMethodAsync<TSwitchValue>> _caseMethods = new();
    private ActivitySwitchMethodAsync<TSwitchValue> _defaultMethod;

    /// <inheritdoc />
    public ActivitySwitch(IActivityInformation activityInformation, ActivityMethodAsync<IActivitySwitch<TSwitchValue>, TSwitchValue> switchValueMethodAsync) 
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(switchValueMethodAsync, nameof(switchValueMethodAsync));
        SwitchValueMethodAsync = switchValueMethodAsync;
    }

    /// <inheritdoc />
    [JsonIgnore]
    public ActivityMethodAsync<IActivitySwitch<TSwitchValue>, TSwitchValue> SwitchValueMethodAsync { get; }

    /// <inheritdoc />
    public IActivitySwitch<TSwitchValue> Case(TSwitchValue caseValue, ActivitySwitchMethodAsync<TSwitchValue> methodAsync)
    {
        InternalContract.Require(!_caseMethods.ContainsKey(caseValue), $"Doublet {nameof(caseValue)}.");
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _caseMethods.TryAdd(caseValue, methodAsync);
        return this;
    }

    /// <inheritdoc />
    public IActivitySwitch<TSwitchValue> Default(ActivitySwitchMethodAsync<TSwitchValue> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _defaultMethod = methodAsync;
        return this;
    }

    internal async Task SwitchAsync(CancellationToken cancellationToken = default)
    {
        var switchValue = await LogicExecutor.ExecuteWithReturnValueAsync(ct => SwitchValueMethodAsync(this, cancellationToken), "Switch", cancellationToken);
        var caseValue = switchValue?.ToString();
        ActivitySwitchMethodAsync<TSwitchValue> methodAsync = null;
        var found = switchValue != null && _caseMethods.TryGetValue(switchValue, out methodAsync);
        if (!found)
        {
            methodAsync = _defaultMethod;
            caseValue = "default";
        }
        if (methodAsync == null) return;
        await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => methodAsync(this, ct), $"Case {caseValue}", cancellationToken);
    }

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(SwitchAsync, cancellationToken);
    }
}

internal class ActivitySwitch<TActivityReturns, TSwitchValue> : 
    Activity<TActivityReturns>,
    IActivitySwitch<TActivityReturns, TSwitchValue>
    where TSwitchValue : IComparable, IComparable<TSwitchValue>
{
    private readonly Dictionary<TSwitchValue, ActivitySwitchMethodAsync<TActivityReturns, TSwitchValue>> _caseMethods = new();
    private ActivitySwitchMethodAsync<TActivityReturns, TSwitchValue> _defaultMethod;

    /// <inheritdoc />
    public ActivitySwitch(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync, ActivityMethodAsync<IActivitySwitch<TActivityReturns, TSwitchValue>, TSwitchValue> switchValueMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(switchValueMethodAsync, nameof(switchValueMethodAsync));
        SwitchValueMethodAsync = switchValueMethodAsync;
    }

    /// <inheritdoc />
    [JsonIgnore]
    public ActivityMethodAsync<IActivitySwitch<TActivityReturns, TSwitchValue>, TSwitchValue> SwitchValueMethodAsync { get; }

    /// <inheritdoc />
    public IActivitySwitch<TActivityReturns, TSwitchValue> Case(TSwitchValue caseValue, ActivitySwitchMethodAsync<TActivityReturns, TSwitchValue> methodAsync)
    {
        InternalContract.Require(!_caseMethods.ContainsKey(caseValue), $"Doublet {nameof(caseValue)}.");
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _caseMethods.TryAdd(caseValue, methodAsync);
        return this;
    }

    /// <inheritdoc />
    public IActivitySwitch<TActivityReturns, TSwitchValue> Case(TSwitchValue caseValue, TActivityReturns value)
    {
        InternalContract.Require(!_caseMethods.ContainsKey(caseValue), $"Doublet {nameof(caseValue)}.");
        _caseMethods.TryAdd(caseValue, (_, _) => Task.FromResult(value));
        return this;
    }

    /// <inheritdoc />
    public IActivitySwitch<TActivityReturns, TSwitchValue> Default(ActivitySwitchMethodAsync<TActivityReturns, TSwitchValue> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _defaultMethod = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IActivitySwitch<TActivityReturns, TSwitchValue> Default(TActivityReturns value)
    {
        _defaultMethod = (_, _) => Task.FromResult(value);
        return this;
    }


    internal async Task<TActivityReturns> SwitchAsync(CancellationToken cancellationToken = default)
    {
        var switchValue = await LogicExecutor.ExecuteWithReturnValueAsync(ct => SwitchValueMethodAsync(this, ct), "Switch", cancellationToken);
        var caseValue = switchValue?.ToString();
        ActivitySwitchMethodAsync<TActivityReturns, TSwitchValue> methodAsync = null;
        var found = switchValue != null && _caseMethods.TryGetValue(switchValue, out methodAsync);
        if (!found)
        {
            methodAsync = _defaultMethod;
            caseValue = "default";
        }

        if (methodAsync == null)
        {
            throw new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowImplementationError, 
                $"The switch value was {switchValue}, but that could not be matched with any of the case values and there was no default catch.",
                "The workflow has a bug in it (a switch with no corresponding case). Please report this to the workflow developer.");
        }

        return await LogicExecutor.ExecuteWithReturnValueAsync(ct => methodAsync!(this, ct), $"Case {caseValue}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(SwitchAsync, DefaultValueMethodAsync ,cancellationToken);
    }
}