﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using static Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes.ActivityAction;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityAction" />
internal class ActivityAction : Activity, IActivityActionWithThrottle, IActivityActionMaybeBackground
{
    private const string SerializedException = nameof(SerializedException);
    private ActivityMethodAsync<IActivityAction> _methodAsync;
    private DateTimeOffset _maxTime = DateTimeOffset.MaxValue;

    [JsonIgnore]
    internal ISemaphoreSupport LockSemaphoreSupport { get; set; }

    [JsonIgnore]
    internal ISemaphoreSupport ThrottleSemaphoreSupport { get; set; }

    protected ActivityMethodAsync<IActivityAction> WhenWaitingAsync { get; set; }

    /// <summary>
    /// The "catch all" method if none of the <see cref="_catchAsyncMethods"/> is applicable.
    /// </summary>
    private TryCatchMethodAsync _catchAllMethodAsync;

    /// <summary>
    /// The "catch" methods for different <see cref="ActivityExceptionCategoryEnum"/>.
    /// </summary>
    private readonly Dictionary<ActivityExceptionCategoryEnum, TryCatchMethodAsync> _catchAsyncMethods = new();

    private bool _trySynchronousHttpRequestFirst;
    private bool _fireAndForget;

    [Obsolete("Please use the constructor with a method parameter. Obsolete since 2022-05-01.")]
    public ActivityAction(IActivityInformation activityInformation)
        : base(activityInformation)
    {
    }

    public ActivityAction(IActivityInformation activityInformation, ActivityMethodAsync<IActivityAction> methodAsync)
        : base(activityInformation)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        _methodAsync = methodAsync;
    }

    /// <inheritdoc />
    public IActivityAction SetMaxTime(DateTimeOffset time)
    {
        _maxTime = time;
        return this;
    }

    /// <inheritdoc />
    public IActivityAction SetMaxTime(TimeSpan timeSpan)
    {
        _maxTime = ActivityStartedAt.Add(timeSpan);
        return this;
    }

    /// <inheritdoc />
    public IActivityAction TrySynchronousHttpRequestFirst()
    {
        _trySynchronousHttpRequestFirst = true;
        return this;
    }

    /// <inheritdoc/>
    [Obsolete(
        "Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task ExecuteAsync(ActivityMethodAsync<IActivityAction> methodAsync,
        CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync == null,
            $"You must use the {nameof(IActivityAction.ExecuteAsync)}() method that has no method parameter.");
        _methodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ct => methodAsync(this, ct), cancellationToken);
    }

    /// <inheritdoc />
    public IExecutableActivity SetFireAndForget()
    {
        _fireAndForget = true;
        return this;
    }

    /// <inheritdoc />
    public ITryCatchActivity Catch(ActivityExceptionCategoryEnum category, TryCatchMethodAsync methodAsync)
    {
        InternalContract.Require(!_catchAsyncMethods.ContainsKey(category),
            $"A catch method for category {category} has already been set for this activity.");
        _catchAsyncMethods.Add(category, methodAsync);
        return this;
    }

    /// <inheritdoc />
    public ITryCatchActivity Catch(ActivityExceptionCategoryEnum category, TryCatchMethod method)
    {
        return Catch(category, (a, e, _) =>
        {
            method(a, e);
            return Task.CompletedTask;
        });
    }

    /// <inheritdoc />
    public IExecutableActivity CatchAll(TryCatchMethodAsync methodAsync)
    {
        InternalContract.Require(_catchAllMethodAsync == null,
            "A catch-all method has already been set for this activity.");
        _catchAllMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity CatchAll(TryCatchMethod method)
    {
        return CatchAll((a, e, _) =>
        {
            method(a, e);
            return Task.CompletedTask;
        });
    }

    /// <inheritdoc />
    public IActivityActionLockOrThrottle UnderLock(string resourceIdentifier)
    {
        LockSemaphoreSupport = new SemaphoreSupport(resourceIdentifier)
        {
            Activity = this
        };
        return this;
    }

    /// <inheritdoc />
    public IActivityActionWithThrottle WithThrottle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan = null)
    {
        ThrottleSemaphoreSupport = new SemaphoreSupport(resourceIdentifier, limit, limitationTimeSpan)
        {
            Activity = this
        };
        return this;
    }

    /// <inheritdoc />
    public ITryCatchActivity WhenWaiting(ActivityMethodAsync<IActivityAction> whenWaitingAsync)
    {
        InternalContract.RequireNotNull(whenWaitingAsync, nameof(whenWaitingAsync));
        WhenWaitingAsync = whenWaitingAsync;
        return this;
    }

    /// <inheritdoc />
    public ITryCatchActivity WhenWaiting(ActivityMethod<IActivityAction> whenWaiting)
    {
        InternalContract.RequireNotNull(whenWaiting, nameof(whenWaiting));
        WhenWaitingAsync = (a, _) =>
        {
            whenWaiting(a);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null,
            $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
        if (_fireAndForget) WorkflowStatic.Context.ExecutionBackgroundStyle = BackgroundStyleEnum.FireAndForget;
        await ActivityExecutor.ExecuteWithoutReturnValueAsync(ActionAsync, cancellationToken);
        await ActivityExecutor.DoExtraAdminAsync(async (ct) =>
        {
            await MaybeLowerAsync(ThrottleSemaphoreSupport, ct);
            await MaybeLowerAsync(LockSemaphoreSupport, ct);
        }, cancellationToken);
    }

    internal async Task ActionAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        var methodName = _catchAllMethodAsync == null && !_catchAsyncMethods.Any() ? "Action" : "Try";
        while (true)
        {
            TryGetContext(SerializedException, out string serializedException);
            if (string.IsNullOrWhiteSpace(serializedException))
            {
                try
                {
                    await LogicExecutor.ExecuteWithoutReturnValueAsync(async ct =>
                    {
                        if (DateTimeOffset.UtcNow > _maxTime)
                        {
                            throw new ActivityFailedException(ActivityExceptionCategoryEnum.MaxTimeReachedError,
                                $"The maximum time ({_maxTime.ToLogString()}) for the activity {ToLogString()} has been reached. The activity was started at {ActivityStartedAt.ToLogString()} and expired at {_maxTime.ToLogString()}, it is now {DateTimeOffset.UtcNow.ToLogString()}",
                                "The maximum time for the activity has been reached.");
                        }
                        await MaybeRaiseAsync(LockSemaphoreSupport, WhenWaitingAsync, cancellationToken);
                        await MaybeRaiseAsync(ThrottleSemaphoreSupport, WhenWaitingAsync, cancellationToken);
                        if (_trySynchronousHttpRequestFirst
                            && Instance.AsyncRequestId == null
                            && WorkflowStatic.Context.ExecutionIsAsynchronous)
                        {
                            try
                            {
                                WorkflowStatic.Context.ExecutionIsAsynchronous = false;
                                await _methodAsync(this, ct);
                            }
                            catch (Exception exception)
                            {
                                await this.LogWarningAsync($"First try with synchronous HTTP request failed", exception, cancellationToken);
                                WorkflowStatic.Context.ExecutionIsAsynchronous = true;
                                await _methodAsync(this, ct);
                            }
                            finally
                            {
                                WorkflowStatic.Context.ExecutionIsAsynchronous = true;
                            }
                        }
                        else
                        {
                            await _methodAsync(this, ct);
                        }
                    }, methodName,
                        cancellationToken);
                    return;
                }
                catch (ActivityFailedException e)
                {
                    await MaybeLowerAsync(ThrottleSemaphoreSupport, cancellationToken);
                    await MaybeLowerAsync(LockSemaphoreSupport, cancellationToken);
                    serializedException = e.Serialize();
                    if (!_catchAsyncMethods.TryGetValue(e.ExceptionCategory, out _)
                        && _catchAllMethodAsync == null)
                    {
                        throw;
                    }

                    Instance.Reset();
                    SetContext(SerializedException, serializedException);
                }
            }
            var exception = ActivityFailedException.Deserialize(serializedException);
            var catchName = exception.ExceptionCategory.ToString();
            if (!_catchAsyncMethods.TryGetValue(exception.ExceptionCategory, out var methodAsync))
            {
                methodAsync = _catchAllMethodAsync;
                catchName = "all";
            }
            FulcrumAssert.IsNotNull(methodAsync, CodeLocation.AsString());
            try
            {
                await LogicExecutor.ExecuteWithoutReturnValueAsync(
                    ct => methodAsync!(this, exception, ct),
                    $"Catch {catchName}",
                    cancellationToken);
                return;
            }
            catch (RetryActivityFromCatchException)
            {
                Instance.Reset();
                // The loop will try the action again
            }
        }
    }

    public enum BackgroundStyleEnum
    {
        None,
        FireAndForget,
        Spawn
    }
}

internal class ActivityAction<TActivityReturns> : Activity<TActivityReturns>, IActivityActionMaybeBackground<TActivityReturns>, IActivityActionWithThrottle<TActivityReturns>
{
    private const string SerializedException = nameof(SerializedException);
    private ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> _methodAsync;
    private DateTimeOffset _maxTime = DateTimeOffset.MaxValue;

    protected ActivityMethodAsync<IActivityAction<TActivityReturns>> WhenWaitingAsync { get; set; }

    [JsonIgnore]
    internal ISemaphoreSupport LockSemaphoreSupport { get; set; }

    [JsonIgnore]
    internal ISemaphoreSupport ThrottleSemaphoreSupport { get; set; }

    /// <summary>
    /// The "catch all" method if none of the <see cref="_catchAsyncMethods"/> is applicable.
    /// </summary>
    private TryCatchMethodAsync<TActivityReturns> _catchAllMethodAsync;

    /// <summary>
    /// The "catch" methods for different <see cref="ActivityExceptionCategoryEnum"/>.
    /// </summary>
    private readonly Dictionary<ActivityExceptionCategoryEnum, TryCatchMethodAsync<TActivityReturns>> _catchAsyncMethods = new();

    private bool _trySynchronousHttpRequestFirst;
    private bool _fireAndForget;

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

    /// <inheritdoc />
    public IActivityAction<TActivityReturns> SetMaxTime(DateTimeOffset time)
    {
        _maxTime = time;
        return this;
    }

    /// <inheritdoc />
    public IActivityAction<TActivityReturns> SetMaxTime(TimeSpan timeSpan)
    {
        _maxTime = ActivityStartedAt.Add(timeSpan);
        return this;
    }

    /// <inheritdoc />
    public IActivityAction<TActivityReturns> TrySynchronousHttpRequestFirst()
    {
        _trySynchronousHttpRequestFirst = true;
        return this;
    }

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method parameter in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public async Task<TActivityReturns> ExecuteAsync(ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync == null, $"You must use the {nameof(IActivityAction.ExecuteAsync)}() method that has no method parameter.");
        _methodAsync = methodAsync;
        return await ActivityExecutor.ExecuteWithReturnValueAsync(ct => methodAsync(this, ct), DefaultValueMethodAsync, cancellationToken);
    }

    /// <inheritdoc />
    public ITryCatchActivity<TActivityReturns> Catch(ActivityExceptionCategoryEnum category, TryCatchMethodAsync<TActivityReturns> methodAsync)
    {
        InternalContract.Require(!_catchAsyncMethods.ContainsKey(category), $"A catch method for category {category} has already been set for this activity.");
        _catchAsyncMethods.Add(category, methodAsync);
        return this;
    }

    /// <inheritdoc />
    public ITryCatchActivity<TActivityReturns> Catch(ActivityExceptionCategoryEnum category, TryCatchMethod<TActivityReturns> method)
    {
        return Catch(category, (a, e, _) => Task.FromResult(method(a, e)));
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> CatchAll(TryCatchMethodAsync<TActivityReturns> methodAsync)
    {
        InternalContract.Require(_catchAllMethodAsync == null, "A catch-all method has already been set for this activity.");
        _catchAllMethodAsync = methodAsync;
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity<TActivityReturns> CatchAll(TryCatchMethod<TActivityReturns> method)
    {
        return CatchAll((a, e, _) => Task.FromResult(method(a, e)));
    }

    /// <inheritdoc />
    public IActivityActionLockOrThrottle<TActivityReturns> UnderLock(string resourceIdentifier)
    {
        LockSemaphoreSupport = new SemaphoreSupport(resourceIdentifier)
        {
            Activity = this
        };
        return this;
    }

    /// <inheritdoc />
    public IActivityActionWithThrottle<TActivityReturns> WithThrottle(string resourceIdentifier, int limit, TimeSpan? limitationTimeSpan = null)
    {
        ThrottleSemaphoreSupport = new SemaphoreSupport(resourceIdentifier, limit, limitationTimeSpan)
        {
            Activity = this
        };
        return this;
    }

    /// <inheritdoc />
    public ITryCatchActivity<TActivityReturns> WhenWaiting(ActivityMethodAsync<IActivityAction<TActivityReturns>> whenWaitingAsync)
    {
        InternalContract.RequireNotNull(whenWaitingAsync, nameof(whenWaitingAsync));
        WhenWaitingAsync = whenWaitingAsync;
        return this;
    }

    /// <inheritdoc />
    public ITryCatchActivity<TActivityReturns> WhenWaiting(ActivityMethod<IActivityAction<TActivityReturns>> whenWaiting)
    {
        InternalContract.RequireNotNull(whenWaiting, nameof(whenWaiting));
        WhenWaitingAsync = (a, _) =>
        {
            whenWaiting(a);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <inheritdoc />
    public IExecutableActivity SetFireAndForget()
    {
        _fireAndForget = true;
        return this;
    }

    /// <inheritdoc />
    public override async Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
        if (_fireAndForget) WorkflowStatic.Context.ExecutionBackgroundStyle = BackgroundStyleEnum.FireAndForget;
        var result = await ActivityExecutor.ExecuteWithReturnValueAsync(ActionAsync, DefaultValueMethodAsync, cancellationToken);
        await ActivityExecutor.DoExtraAdminAsync(async (ct) =>
        {
            await MaybeLowerAsync(ThrottleSemaphoreSupport, ct);
            await MaybeLowerAsync(LockSemaphoreSupport, ct);
        }, cancellationToken);
        return result;
    }

    internal async Task<TActivityReturns> ActionAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        var methodName = _catchAllMethodAsync == null && !_catchAsyncMethods.Any() ? "Action" : "Try";
        while (true)
        {
            TryGetContext(SerializedException, out string serializedException);
            if (string.IsNullOrWhiteSpace(serializedException))
            {
                try
                {
                    var result = await LogicExecutor.ExecuteWithReturnValueAsync(async ct =>
                    {
                        if (DateTimeOffset.UtcNow > _maxTime)
                        {
                            throw new ActivityFailedException(ActivityExceptionCategoryEnum.MaxTimeReachedError,
                                $"The maximum time ({_maxTime.ToLogString()}) for the activity {ToLogString()} has been reached. The activity was started at {ActivityStartedAt.ToLogString()} and expired at {_maxTime.ToLogString()}, it is now {DateTimeOffset.UtcNow.ToLogString()}",
                                "The maximum time for the activity has been reached.");
                        }
                        await MaybeRaiseAsync(ThrottleSemaphoreSupport, WhenWaitingAsync, cancellationToken);
                        await MaybeRaiseAsync(LockSemaphoreSupport, WhenWaitingAsync, cancellationToken);
                        TActivityReturns returnValue;
                        if (_trySynchronousHttpRequestFirst
                            && Instance.AsyncRequestId == null
                            && WorkflowStatic.Context.ExecutionIsAsynchronous)
                        {
                            try
                            {
                                WorkflowStatic.Context.ExecutionIsAsynchronous = false;
                                returnValue = await _methodAsync(this, ct);
                            }
                            catch (Exception exception)
                            {
                                await this.LogWarningAsync($"First try with synchronous HTTP request failed", exception, cancellationToken);
                                WorkflowStatic.Context.ExecutionIsAsynchronous = true;
                                returnValue = await _methodAsync(this, ct);
                            }
                            finally
                            {
                                WorkflowStatic.Context.ExecutionIsAsynchronous = true;
                            }
                        }
                        else
                        {
                            returnValue = await _methodAsync(this, ct);
                        }
                        return returnValue;
                    }, methodName,
                        cancellationToken);
                    return result;
                }
                catch (ActivityFailedException e)
                {
                    await MaybeLowerAsync(LockSemaphoreSupport, cancellationToken);
                    await MaybeLowerAsync(ThrottleSemaphoreSupport, cancellationToken);
                    serializedException = e.Serialize();
                    if (!_catchAsyncMethods.TryGetValue(e.ExceptionCategory, out _)
                        && _catchAllMethodAsync == null)
                    {
                        throw;
                    }

                    Instance.Reset();
                    SetContext(SerializedException, serializedException);
                }
            }
            var exception = ActivityFailedException.Deserialize(serializedException);
            var catchName = exception.ExceptionCategory.ToString();
            if (!_catchAsyncMethods.TryGetValue(exception.ExceptionCategory, out var methodAsync))
            {
                methodAsync = _catchAllMethodAsync;
                catchName = "all";
            }
            FulcrumAssert.IsNotNull(methodAsync, CodeLocation.AsString());
            try
            {
                var result = await LogicExecutor.ExecuteWithReturnValueAsync(
                    ct => methodAsync!(this, exception, ct),
                    $"Catch {catchName}",
                    cancellationToken);
                return result;
            }
            catch (RetryActivityFromCatchException)
            {
                Instance.Reset();
                // The loop will try the action again
            }
        }
    }
}