using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityAction" />
internal class ActivityAction : Activity, IActivityAction, IBackgroundActivity
{
    private readonly ActivityMethodAsync<IActivityAction> _methodAsync;

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityAction(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    public ActivityAction(IActivityInformation activityInformation, ActivityMethodAsync<IActivityAction> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task ExecuteAsync(ActivityMethodAsync<IActivityAction> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync == null, $"You must use the {nameof(IActivityAction.ExecuteAsync)}() method that has no method parameter.");
        return ActivityExecutor.ExecuteWithoutReturnValueAsync( ct => methodAsync(this, ct), cancellationToken);
    }

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => _methodAsync(this, ct), cancellationToken);
    }
}

internal class ActivityAction<TActivityReturns> : Activity<TActivityReturns>, IActivityAction<TActivityReturns>, IBackgroundActivity<TActivityReturns>
{
    private readonly ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> _methodAsync;

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityAction(IActivityInformation activityInformation, 
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
    }
    public ActivityAction(IActivityInformation activityInformation, 
        ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync,
        ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync == null, $"You must use the {nameof(IActivityAction<TActivityReturns>.ExecuteAsync)}() method that has no method parameter.");
        return ActivityExecutor.ExecuteWithReturnValueAsync( ct => methodAsync(this, ct), DefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
        return ActivityExecutor.ExecuteWithReturnValueAsync(ct => _methodAsync(this, ct), DefaultValueMethodAsync, cancellationToken);
    }
}