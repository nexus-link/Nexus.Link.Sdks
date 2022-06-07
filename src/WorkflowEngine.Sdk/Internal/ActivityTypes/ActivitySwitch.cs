﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


/// <inheritdoc cref="IActivitySwitch{TSwitchValue}" />
internal class ActivitySwitch<TSwitchValue> : Activity, IActivitySwitch<TSwitchValue>
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
        var switchValue = await SwitchValueMethodAsync(this, cancellationToken);
        var found = _caseMethods.TryGetValue(switchValue, out var methodAsync);
        if (!found) methodAsync = _defaultMethod;
        if (methodAsync == null) return;
        await methodAsync(this, cancellationToken);
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
        var switchValue = await SwitchValueMethodAsync(this, cancellationToken);
        var found = _caseMethods.TryGetValue(switchValue, out var methodAsync);
        if (!found) methodAsync = _defaultMethod;
        InternalContract.Require(methodAsync != null, $"Could not match {switchValue} with any of the case values.");
        return await methodAsync!(this, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(SwitchAsync, DefaultValueMethodAsync ,cancellationToken);
    }
}