using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;

/// <inheritdoc cref="IActivityAction" />
internal class ActivityAction : Activity, IActivityAction
{
    private ActivityMethodAsync<IActivityAction> _methodAsync;

    /// <summary>
    /// The "catch all" method if none of the <see cref="_catchAsyncMethods"/> is applicable.
    /// </summary>
    private TryCatchMethodAsync _catchAllMethodAsync;

    /// <summary>
    /// The "catch" methods for different <see cref="ActivityExceptionCategoryEnum"/>.
    /// </summary>
    private readonly Dictionary<ActivityExceptionCategoryEnum, TryCatchMethodAsync> _catchAsyncMethods = new();

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

    /// <inheritdoc/>
    [Obsolete("Please use the ExecuteAsync() method without a method in concert with the constructor that has a method parameter. Obsolete since 2022-05-01.")]
    public Task ExecuteAsync(ActivityMethodAsync<IActivityAction> methodAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync == null, $"You must use the {nameof(IActivityAction.ExecuteAsync)}() method that has no method parameter.");
        _methodAsync = methodAsync;
        return ActivityExecutor.ExecuteWithoutReturnValueAsync( ct => methodAsync(this, ct), cancellationToken);
    }

    /// <inheritdoc />
    public ITryCatchActivity Catch(ActivityExceptionCategoryEnum category, TryCatchMethodAsync methodAsync)
    {
        InternalContract.Require(!_catchAsyncMethods.ContainsKey(category), $"A catch method for category {category} has already been set for this activity.");
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
        InternalContract.Require(_catchAllMethodAsync == null, "A catch-all method has already been set for this activity.");
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
    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
        return ActivityExecutor.ExecuteWithoutReturnValueAsync(ActionAsync, cancellationToken);
    }

    internal async Task ActionAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        var methodName = _catchAllMethodAsync == null && !_catchAsyncMethods.Any() ? "Action" : "Try";
        try
        {
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => _methodAsync(this, ct), methodName, cancellationToken);
        }
        catch (ActivityFailedException e)
        {
            var catchName = e.ExceptionCategory.ToString();
            if (!_catchAsyncMethods.TryGetValue(e.ExceptionCategory, out var methodAsync))
            {
                methodAsync = _catchAllMethodAsync;
                catchName = "all";
            }

            if (methodAsync == null) throw;
            await LogicExecutor.ExecuteWithoutReturnValueAsync(ct => methodAsync(this, e, ct), $"Catch {catchName}",
                cancellationToken);
        }
    }
}

internal class ActivityAction<TActivityReturns> : Activity<TActivityReturns>, IActivityAction<TActivityReturns>
{
    private ActivityMethodAsync<IActivityAction<TActivityReturns>, TActivityReturns> _methodAsync;

    /// <summary>
    /// The "catch all" method if none of the <see cref="_catchAsyncMethods"/> is applicable.
    /// </summary>
    private TryCatchMethodAsync<TActivityReturns> _catchAllMethodAsync;

    /// <summary>
    /// The "catch" methods for different <see cref="ActivityExceptionCategoryEnum"/>.
    /// </summary>
    private readonly Dictionary<ActivityExceptionCategoryEnum, TryCatchMethodAsync<TActivityReturns>> _catchAsyncMethods = new();

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
    public async Task<TActivityReturns> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        InternalContract.Require(_methodAsync != null, $"You must use the {nameof(IActivityFlow.Action)}() method that has a method as parameter.");
         return await ActivityExecutor.ExecuteWithReturnValueAsync(ActionAsync, DefaultValueMethodAsync, cancellationToken);
    }

    internal async Task<TActivityReturns> ActionAsync(CancellationToken cancellationToken = default)
    {
        FulcrumAssert.IsNotNull(_methodAsync, CodeLocation.AsString());
        var methodName = _catchAllMethodAsync == null && !_catchAsyncMethods.Any() ? "Action" : "Try";
        try
        {
            var result = await LogicExecutor.ExecuteWithReturnValueAsync(ct => _methodAsync(this, ct), methodName,
                cancellationToken);
            return result;
        }
        catch (ActivityFailedException e)
        {
            var catchName = e.ExceptionCategory.ToString();
            if (!_catchAsyncMethods.TryGetValue(e.ExceptionCategory, out var methodAsync))
            {
                methodAsync = _catchAllMethodAsync;
                catchName = "default";
            }

            if (methodAsync == null) throw;

            var result =
                await LogicExecutor.ExecuteWithReturnValueAsync(ct => methodAsync(this, e, ct), $"Catch {catchName}",
                    cancellationToken);
            return result;
        }
    }
}