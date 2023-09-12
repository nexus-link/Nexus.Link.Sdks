using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
internal class ActivityExecutor : IActivityExecutor
{
    public IInternalActivity Activity { get; }

    public ActivityExecutor(IInternalActivity activity)
    {
        Activity = activity;
        InternalContract.RequireNotNull(activity, nameof(activity));
    }

    /// <inheritdoc />
    public async Task ExecuteWithoutReturnValueAsync(InternalActivityMethodAsync methodAsync,
        CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

        await SafeExecuteAsync(async ct => { await methodAsync(ct); return true; }, false, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TActivityReturns> ExecuteWithReturnValueAsync<TActivityReturns>(InternalActivityMethodAsync<TActivityReturns> methodAsync,
        ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

        return await SafeExecuteAsync(methodAsync, true, getDefaultValueAsync, cancellationToken);
    }

    /// <summary>
    /// The single purpose of this method is to verify that we limit the type of exceptions that can be thrown, both outer and inner exception.
    /// </summary>
    /// <exception cref="WorkflowImplementationShouldNotCatchThisException"></exception>
    private async Task<TActivityReturns> SafeExecuteAsync<TActivityReturns>(
        InternalActivityMethodAsync<TActivityReturns> methodAsync,
        bool hasReturnValue, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync,
        CancellationToken cancellationToken = default)
    {
        Exception exception;
        try
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            (var result, exception) = await FastForwardOrExecuteAsync(methodAsync, hasReturnValue, getDefaultValueAsync,
                cancellationToken);
            if (exception == null) return result;
        }
        catch (Exception e)
        {
            exception = e;
        }
        finally
        {
            try
            {
                Activity.PromoteOrPurgeLogs();
            }
            catch (Exception e)
            {
                // Ignore logging problems unless we are in development mode
                if (FulcrumApplication.IsInDevelopment)
                {
                    throw new FulcrumAssertionFailedException(
                        $"Unexpected exception when calling {nameof(Activity.PromoteOrPurgeLogs)}. {e.GetType().Name}: {e.Message}")
                    {
                        ErrorLocation = CodeLocation.AsString()
                    };
                }
            }
        }

        switch (exception)
        {
            case WorkflowImplementationShouldNotCatchThisException:
                throw exception;
            case ActivityFailedException
                      or WorkflowFastForwardBreakException
                      or RequestPostponedException
#pragma warning disable CS0618
                      or IgnoreAndExitToParentException:
#pragma warning restore CS0618
                {
                    throw new WorkflowImplementationShouldNotCatchThisException(exception);
                }
            default:
                // Unexpected exception. Our conclusion is that there is an error in the workflow engine.
                var unexpectedException = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                    $"The workflow engine threw an unexpected exception when executing a activity's logic method. This is the exception:\r{exception}",
                    $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
                throw new WorkflowImplementationShouldNotCatchThisException(unexpectedException);
        }
       
    }

    private async Task<(TActivityReturns, Exception)> FastForwardOrExecuteAsync<TActivityReturns>(InternalActivityMethodAsync<TActivityReturns> methodAsync,
        bool hasReturnValue, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync,
        CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

        Exception exception = null;
        if (Activity.Instance.HasCompleted)
        {
            await Activity.LogVerboseAsync($"Fast forward over {Activity.ToLogString()}.", Activity.Instance, cancellationToken);
        }
        else
        {
            exception = await LogAndExecuteAsync(methodAsync, hasReturnValue, cancellationToken);
            if (exception != null) return (default, exception);
            FulcrumAssert.IsTrue(Activity.Instance.HasCompleted, CodeLocation.AsString());
        }
        exception = EvaluateFailUrgencyAndMaybeCreateException();
        if (exception != null) return (default, exception);
        var result = hasReturnValue ? await GetResult() : default;
        return (result, null);

        Exception EvaluateFailUrgencyAndMaybeCreateException()
        {
            if (Activity.Instance.State != ActivityStateEnum.Failed) return null;
            switch (Activity.Options.FailUrgency)
            {
                case ActivityFailUrgencyEnum.CancelWorkflow:
                    return new WorkflowFailedException(
                        Activity.Instance.ExceptionCategory!.Value,
                        $"Activity {Activity}:\r{Activity.Instance.ExceptionTechnicalMessage}",
                        $"Activity {Activity}:\r{Activity.Instance.ExceptionFriendlyMessage}");
                case ActivityFailUrgencyEnum.Stopping:
                    return new ActivityPostponedException(null);
                case ActivityFailUrgencyEnum.HandleLater:
                case ActivityFailUrgencyEnum.Ignore:
                    return null;
                default:
                    return new FulcrumAssertionFailedException(
                        $"Unexpected {nameof(ActivityFailUrgencyEnum)} value: {Activity.Version.FailUrgency}.",
                        CodeLocation.AsString());
            }
        }

        async Task<TActivityReturns> GetResult()
        {
            TActivityReturns result;
            if (Activity.Instance.State == ActivityStateEnum.Success)
            {
                var resultActivity = Activity as IInternalActivity<TActivityReturns>;
                FulcrumAssert.IsNotNull(resultActivity, CodeLocation.AsString());
                result = resultActivity!.GetResult();
            }
            else
            {
                // We will get here if the activity failed and the activity was configured to ignore the failure
                result = await GetDefaultValueAsync();
            }

            return result;
        }

        async Task<TActivityReturns> GetDefaultValueAsync()
        {
            if (getDefaultValueAsync == null) return default;
            try
            {
                return await getDefaultValueAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await Activity.LogErrorAsync(
                    $"The default value method for activity {Activity.ToLogString()} threw an exception." +
                    $" The default value for {typeof(TActivityReturns).Name} ({default(TActivityReturns)}) is used instead.",
                    e, cancellationToken);
                return default;
            }
        }
    }

    private async Task<Exception> LogAndExecuteAsync<TActivityReturns>(InternalActivityMethodAsync<TActivityReturns> methodAsync,
        bool hasReturnValue, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await Activity.LogVerboseAsync($"Begin activity {Activity.ToLogString()}.", Activity.Instance, cancellationToken);
        TActivityReturns result = default;
        var hasResult = false;
        Exception exception = null;
        try
        {
            exception = await ExecuteAndUpdateActivityInformationAsync(methodAsync, hasReturnValue, stopwatch, cancellationToken);
            hasResult = exception == null && Activity.Instance.State == ActivityStateEnum.Success && hasReturnValue;
        }
        finally
        {
            if (Activity.Instance.State == ActivityStateEnum.Failed)
            {
                await Activity.LogWarningAsync($"Activity {Activity.ToLogString()} failed with category {Activity.Instance.ExceptionCategory}",
                    new
                    {
                        ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"),
                        ActivityFailedException = new
                        {
                            Category = Activity.Instance.ExceptionCategory,
                            TechnicalMessage = Activity.Instance.ExceptionTechnicalMessage,
                            FriendlyMessage = Activity.Instance.ExceptionFriendlyMessage
                        }
                    },
                    cancellationToken);
            }
            stopwatch.Stop();
            await Activity.LogVerboseAsync($"End activity {Activity.ToLogString()}.",
                hasResult
                    ? new { ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"), Result = result, ActivityInstance = Activity.Instance }
                    : new { ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"), ActivityInstance = Activity.Instance },
                cancellationToken);
        }

        return exception;
    }

    private async Task<Exception> ExecuteAndUpdateActivityInformationAsync<TActivityReturns>(
        InternalActivityMethodAsync<TActivityReturns> methodAsync, bool hasReturnValue, Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {

        var (result, exception) = await ExecuteAndConsolidateExceptionsAsync(methodAsync, cancellationToken);
        if (exception == null)
        {
            if (hasReturnValue)
            {
                Activity.MarkAsSuccess(result);
            }
            else
            {
                Activity.MarkAsSuccess();
            }

            return null;
        }
        switch (exception)
        {
            case WorkflowImplementationShouldNotCatchThisException:
                Activity.Instance.State = ActivityStateEnum.Waiting;
                return exception;
            case WorkflowFastForwardBreakException:
#pragma warning disable CS0618
            case IgnoreAndExitToParentException:
#pragma warning restore CS0618
                return exception;
            case RequestPostponedException e:
                Activity.Instance.State = ActivityStateEnum.Waiting;
                if (e.WaitingForRequestIds.Count == 1)
                {
                    Activity.Instance.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
                }
                return exception;
            case ActivityFailedException e: // Will also catch WorkflowFailedException
#pragma warning disable CS0618
                if (Activity.Options.ExceptionHandler != null)
                {
                    var exitToParent = await Activity.Options.ExceptionHandler(Activity, e, cancellationToken);
                    if (exitToParent) return new IgnoreAndExitToParentException(e);
                }
#pragma warning restore CS0618
                Activity.MarkAsFailed(e);
                await Activity.SafeAlertExceptionAsync(cancellationToken);
                await Activity.LogWarningAsync($"Activity {Activity.ToLogString()} failed: {e.GetType().Name} {e.ExceptionCategory} {e.Message}",
                    new
                    {
                        ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"),
                        Exception = new
                        {
                            TypeName = e.GetType().Name,
                            e.Message
                        },
                        ActivityInstance = Activity.Instance
                    },
                    cancellationToken);
                return e is WorkflowFailedException ? e : null;
            default:
                // Unexpected exception. The consolidation of exceptions must have failed. This is considered a workflow engine error.
                var unexpectedException = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                            $"The workflow engine did not expect the following exception in {CodeLocation.AsString()}:\r{exception}",
                            $"The workflow engine encountered an unexpected error for workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\"." +
                            " Please contact the workflow developer.");
                Activity.MarkAsFailed(unexpectedException);
                await Activity.SafeAlertExceptionAsync(cancellationToken);
                return null;
        }
    }

    private async Task<(TActivityReturns, Exception)> ExecuteAndConsolidateExceptionsAsync<TActivityReturns>(
            InternalActivityMethodAsync<TActivityReturns> methodAsync,
            CancellationToken cancellationToken)
    {
        try
        {
            var result = await ExecuteUnderTimeLimitsAsync(methodAsync, cancellationToken);
            return (result, null);
        }
        catch (Exception ex)
        {
            Exception exception;
            switch (ex)
            {
                case ActivityFailedException: // Also covers WorkflowFailedException
                case WorkflowImplementationShouldNotCatchThisException:
                case RequestPostponedException:
                case WorkflowFastForwardBreakException:
                    exception = ex;
                    break;
                case OperationCanceledException:
                    exception = new ActivityPostponedException(TimeSpan.Zero);
                    break;
                default:
                    // Unexpected exception. Our conclusion is that there is an error in the workflow engine.
                    exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                        $"The workflow engine threw an unexpected exception when executing an activity. This is the exception:\r{ex}",
                        $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
                    break;
            }
            return (default, exception);
        }
    }

    /// <summary>
    /// The single purpose of this method to detect if we have passed any time limits, and in that case interrupt before we even try to go in to the activity code.
    /// </summary>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="ActivityFailedException"></exception>
    private async Task<TActivityReturns> ExecuteUnderTimeLimitsAsync<TActivityReturns>(
        InternalActivityMethodAsync<TActivityReturns> methodAsync, CancellationToken cancellationToken)
    {
        // We will fail if the total execution time limit has passed
#pragma warning disable CS0618
        GetTotalExecutionTimeRemainingOrThrow();
#pragma warning restore CS0618

        // We will postpone if the current run has passed the postpone limit
        var currentRunTimeSoFar = Activity.ActivityInformation.Workflow.TimeSinceCurrentRunStarted.Elapsed;
        var postponeAfter = Activity.ActivityInformation.Options.PostponeAfterTimeSpan;
        if (currentRunTimeSoFar > postponeAfter)
        {
            await Activity.LogVerboseAsync($"The workflow execution has run for {currentRunTimeSoFar.TotalSeconds:F2} s," +
                                           $" passing the limit of {postponeAfter.TotalSeconds:F2} s.", Activity, cancellationToken);
            throw new OperationCanceledException("This exception will eventually be converted to a RequestPostponed exception.");
        }

        // We will postpone if the reduced total execution time has passed
        // This should normally not happen, if the postpone limit above has been set to a low enough value
        Activity.ActivityInformation.Workflow.ReducedTimeCancellationToken.ThrowIfCancellationRequested();
        return await methodAsync(cancellationToken);

        [Obsolete($"Please use Action with {nameof(IActivityAction.SetMaxTime)}. Obsolete since 2023-09-12.")]
        TimeSpan? GetTotalExecutionTimeRemainingOrThrow()
        {
            if (!Activity.ActivityInformation.Options.ActivityMaxExecutionTimeSpan.HasValue) return null;
            var maxExecutionTime = Activity.ActivityInformation.Options.ActivityMaxExecutionTimeSpan.Value;
            var startedAt = Activity.ActivityStartedAt;
            var now = DateTimeOffset.UtcNow;
            var executionTimeSoFar = now - startedAt;
            var timeLeft = maxExecutionTime - executionTimeSoFar;
            if (timeLeft < TimeSpan.Zero)
            {
                throw new ActivityFailedException(ActivityExceptionCategoryEnum.MaxTimeReachedError,
                    $"The maximum time ({maxExecutionTime}) for the activity {Activity.ToLogString()} has been reached. The activity was started at {startedAt.ToLogString()} and expired at {startedAt.Add(maxExecutionTime).ToLogString()}, it is now {now.ToLogString()}",
                    "The maximum time for the activity has been reached.");
            }
            return timeLeft;
        }
    }
}
