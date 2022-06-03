using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;


internal abstract class ActivityLockOrThrottle<TLockOrThrottle, TLockOrThrottleThen> : Activity, IExecutableActivity
    where TLockOrThrottleThen : class where TLockOrThrottle : class
{
    protected ISemaphoreSupport SemaphoreSupport { get; }
    protected ActivityMethodAsync<TLockOrThrottle> ThenMethodAsync { get; set; }
    protected ActivityMethodAsync<TLockOrThrottle> ElseMethodAsync { get; set; }

    public ActivityLockOrThrottle(IActivityInformation activityInformation, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
        SemaphoreSupport = semaphoreSupport;
        SemaphoreSupport.Activity = this;
        SetContext("elseChosen", false);
    }
    
    public string ResourceIdentifier => SemaphoreSupport.ResourceIdentifier;

    public TLockOrThrottleThen Then(ActivityMethodAsync<TLockOrThrottle> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return this as TLockOrThrottleThen;
    }

    public TLockOrThrottleThen Then(ActivityMethod<TLockOrThrottle> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        ThenMethodAsync = (a, _) =>
        {
            method(a);
            return Task.CompletedTask;
        };
        return this as TLockOrThrottleThen;
    }
    
    public IExecutableActivity Else(ActivityMethodAsync<TLockOrThrottle> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ElseMethodAsync = methodAsync;
        return this;
    }
    
    public IExecutableActivity Else(ActivityMethod<TLockOrThrottle> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        ElseMethodAsync = (a, _) =>
        {
            method(a);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(LockOrThrottleAsync, cancellationToken);
    }

    protected internal async Task LockOrThrottleAsync(CancellationToken cancellationToken)
    {
        var elseChosen = GetContext<bool>("elseChosen");
        if (elseChosen)
        {
            await ElseMethodAsync(this as TLockOrThrottle, cancellationToken);
            return;
        }
        try
        {
            await SemaphoreSupport.RaiseAsync(cancellationToken);
        }
        catch (RequestPostponedException)
        {
            if (ElseMethodAsync == null) throw;
            SetContext("elseChosen", true);
            await ElseMethodAsync(this as TLockOrThrottle, cancellationToken);
            return;
        }
        await ThenMethodAsync(this as TLockOrThrottle, cancellationToken);
        await SemaphoreSupport.LowerAsync(cancellationToken);
    }
}

internal class ActivityLockOrThrottle<TActivityReturns, TLockOrThrottle, TLockOrThrottleThen> : Activity<TActivityReturns>, IExecutableActivity<TActivityReturns>
    where TLockOrThrottleThen : class where TLockOrThrottle : class
{
    protected ISemaphoreSupport SemaphoreSupport { get; }
    protected ActivityMethodAsync<TLockOrThrottle, TActivityReturns> ThenMethodAsync;
    protected ActivityMethodAsync<TLockOrThrottle, TActivityReturns> ElseMethodAsync;

    public ActivityLockOrThrottle(IActivityInformation activityInformation, ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync, ISemaphoreSupport semaphoreSupport)
        : base(activityInformation, defaultValueMethodAsync)
    {
        InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
        SemaphoreSupport = semaphoreSupport;
        SemaphoreSupport.Activity = this;
        SetContext("elseChosen", false);
    }

    public string ResourceIdentifier => SemaphoreSupport.ResourceIdentifier;

    public TLockOrThrottleThen Then(ActivityMethodAsync<TLockOrThrottle, TActivityReturns> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ThenMethodAsync = methodAsync;
        return this as TLockOrThrottleThen;
    }

    public TLockOrThrottleThen Then(ActivityMethod<TLockOrThrottle, TActivityReturns> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        ThenMethodAsync = (a, _) => Task.FromResult(method(a));
        return this as TLockOrThrottleThen;
    }

    public IExecutableActivity<TActivityReturns> Else(ActivityMethodAsync<TLockOrThrottle, TActivityReturns> methodAsync)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        ElseMethodAsync = methodAsync;
        return this;
    }

    public IExecutableActivity<TActivityReturns> Else(ActivityMethod<TLockOrThrottle, TActivityReturns> method)
    {
        InternalContract.RequireNotNull(method, nameof(method));
        ElseMethodAsync = (a, _) => Task.FromResult(method(a));
        return this;
    }

    public Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ActivityExecutor.ExecuteWithReturnValueAsync(LockOrThrottleAsync, DefaultValueMethodAsync, cancellationToken);
    }

    protected async Task<TActivityReturns> LockOrThrottleAsync(CancellationToken cancellationToken)
    {
        var elseChosen = GetContext<bool>("elseChosen");
        if (elseChosen)
        {
            return await ElseMethodAsync(this as TLockOrThrottle, cancellationToken);
        }
        try
        {
            await SemaphoreSupport.RaiseAsync(cancellationToken);
        }
        catch (RequestPostponedException)
        {
            if (ElseMethodAsync == null) throw;
            SetContext("elseChosen", true);
            return await ElseMethodAsync(this as TLockOrThrottle, cancellationToken);
        }
        var result = await ThenMethodAsync(this as TLockOrThrottle, cancellationToken);
        await SemaphoreSupport.LowerAsync(cancellationToken);
        return result;
    }
}