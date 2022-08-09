using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
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
        try
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            return await FastForwardOrExecuteAsync(methodAsync, hasReturnValue, getDefaultValueAsync,
                cancellationToken);
        }
        catch (Exception e)
            when (e is ActivityFailedException
                      or WorkflowFastForwardBreakException
                      or RequestPostponedException
#pragma warning disable CS0618
                      or IgnoreAndExitToParentException)
#pragma warning restore CS0618
        {
            throw new WorkflowImplementationShouldNotCatchThisException(e);
        }
        catch (Exception e)
        {
            // Unexpected exception. Our conclusion is that there is an error in the workflow engine.
            var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                $"The workflow engine threw an unexpected exception when executing a activity's logic method. This is the exception:\r{e}",
                $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
            throw new WorkflowImplementationShouldNotCatchThisException(exception);
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
    }

    private async Task<TActivityReturns> FastForwardOrExecuteAsync<TActivityReturns>(InternalActivityMethodAsync<TActivityReturns> methodAsync,
        bool hasReturnValue, ActivityDefaultValueMethodAsync<TActivityReturns> getDefaultValueAsync,
        CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

        if (Activity.Instance.HasCompleted)
        {
            await Activity.LogVerboseAsync($"Fast forward over {Activity.ToLogString()}.", Activity.Instance, cancellationToken);
        }
        else
        {
            await LogAndExecuteAsync(methodAsync, hasReturnValue, cancellationToken);
            FulcrumAssert.IsTrue(Activity.Instance.HasCompleted, CodeLocation.AsString());
        }
        EvaluateFailUrgencyAndMaybeThrow();
        if (!hasReturnValue) return default;
        return await GetResult();

        void EvaluateFailUrgencyAndMaybeThrow()
        {
            if (Activity.Instance.State != ActivityStateEnum.Failed) return;
            switch (Activity.Options.FailUrgency)
            {
                case ActivityFailUrgencyEnum.CancelWorkflow:
                    throw new WorkflowFailedException(
                        Activity.Instance.ExceptionCategory!.Value,
                        $"Activity {Activity}:\r{Activity.Instance.ExceptionTechnicalMessage}",
                        $"Activity {Activity}:\r{Activity.Instance.ExceptionFriendlyMessage}");
                case ActivityFailUrgencyEnum.Stopping:
                    throw new RequestPostponedException();
                case ActivityFailUrgencyEnum.HandleLater:
                case ActivityFailUrgencyEnum.Ignore:
                    return;
                default:
                    throw new FulcrumAssertionFailedException(
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

    private async Task LogAndExecuteAsync<TActivityReturns>(InternalActivityMethodAsync<TActivityReturns> methodAsync,
        bool hasReturnValue, CancellationToken cancellationToken)
    {
        InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await Activity.LogVerboseAsync($"Begin activity {Activity.ToLogString()}.", Activity.Instance, cancellationToken);
        TActivityReturns result = default;
        var hasResult = false;
        try
        {
            await ExecuteAndUpdateActivityInformationAsync(methodAsync, hasReturnValue, stopwatch, cancellationToken);
            hasResult = Activity.Instance.State == ActivityStateEnum.Success && hasReturnValue;
        }
        finally
        {
            if (Activity.Instance.State == ActivityStateEnum.Failed)
            {
                await Activity.LogWarningAsync($"Activity {Activity.ToLogString()} failed with category {Activity.Instance.ExceptionCategory}",
                    new
                    {
                        ElapsedTime = stopwatch.Elapsed.ToLogString(),
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
                    ? new { ElapsedTime = stopwatch.Elapsed.ToLogString(), Result = result, ActivityInstance = Activity.Instance }
                    : new { ElapsedTime = stopwatch.Elapsed.ToLogString(), ActivityInstance = Activity.Instance },
                cancellationToken);
        }
    }

    private async Task ExecuteAndUpdateActivityInformationAsync<TActivityReturns>(
        InternalActivityMethodAsync<TActivityReturns> methodAsync, bool hasReturnValue, Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await ExecuteAndConsolidateExceptionsAsync(methodAsync, cancellationToken);
            if (hasReturnValue)
            {
                Activity.MarkAsSuccess(result);
            }
            else
            {
                Activity.MarkAsSuccess();
            }
        }
        catch (WorkflowFastForwardBreakException)
        {
            throw;
        }
#pragma warning disable CS0618
        catch (IgnoreAndExitToParentException)
        {
            throw;
        }
#pragma warning restore CS0618
        catch (RequestPostponedException e)
        {
            Activity.Instance.State = ActivityStateEnum.Waiting;
            if (e.WaitingForRequestIds.Count == 1)
            {
                Activity.Instance.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
            }
            throw;
        }
        catch (ActivityFailedException e) // Will also catch WorkflowFailedException
        {
#pragma warning disable CS0618
            if (Activity.Options.ExceptionHandler != null)
            {
                var exitToParent = await Activity.Options.ExceptionHandler(Activity, e, cancellationToken);
                if (exitToParent) throw new IgnoreAndExitToParentException(e);
            }
#pragma warning restore CS0618
            Activity.MarkAsFailed(e);
            await Activity.SafeAlertExceptionAsync(cancellationToken);
            await Activity.LogWarningAsync($"Activity {Activity.ToLogString()} failed: {e.GetType().Name} {e.ExceptionCategory} {e.Message}",
                new
                {
                    ElapsedTime = stopwatch.Elapsed.ToLogString(),
                    Exception = new
                    {
                        TypeName = e.GetType().Name,
                        e.Message
                    },
                    ActivityInstance = Activity.Instance
                },
                cancellationToken);
            if (e is WorkflowFailedException) throw;
        }
        catch (Exception e)
        {
            // Unexpected exception. The consolidation of exceptions must have failed. This is considered a workflow engine error.
            var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                        $"The workflow engine did not expect the following exception in {CodeLocation.AsString()}:\r{e}",
                        $"The workflow engine encountered an unexpected error for workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\"." +
                        " Please contact the workflow developer.");
            Activity.MarkAsFailed(exception);
            await Activity.SafeAlertExceptionAsync(cancellationToken);
        }
    }

    private async Task<TActivityReturns> ExecuteAndConsolidateExceptionsAsync<TActivityReturns>(
            InternalActivityMethodAsync<TActivityReturns> methodAsync,
            CancellationToken cancellationToken)
    {
        try
        {
            FulcrumApplication.Context.AsyncPriority = Activity.Options.AsyncRequestPriority;
            FulcrumApplication.Context.AsyncRequestId = Activity.Instance?.AsyncRequestId;
            FulcrumApplication.Context.ChildRequestDescription = Activity.ActivityTitle;
            return await ExecuteUnderTimeLimitsAsync(methodAsync, cancellationToken);
        }
        catch (ActivityFailedException) // Also covers WorkflowFailedException
        {
            throw;
        }
        catch (RequestPostponedException)
        {
            throw;
        }
        catch (WorkflowFastForwardBreakException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw new RequestPostponedException
            {
                TryAgain = true,
                TryAgainAfterMinimumTimeSpan = TimeSpan.Zero
            };
        }
        catch (Exception e)
        {
            // Unexpected exception. Our conclusion is that there is an error in the workflow engine.
            var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                $"The workflow engine threw an unexpected exception when executing an activity. This is the exception:\r{e}",
                $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
            throw exception;
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
        GetTotalExecutionTimeRemainingOrThrow();

        // We will postpone if the current run has passed the postpone limit
        var currentRunTimeSoFar = Activity.ActivityInformation.Workflow.TimeSinceCurrentRunStarted.Elapsed;
        var postponeAfter = Activity.ActivityInformation.Options.PostponeAfterTimeSpan;
        if (currentRunTimeSoFar > postponeAfter)
        {
            await Activity.LogVerboseAsync($"The workflow execution has run for {currentRunTimeSoFar.ToLogString()}," +
                                           $" passing the limit of {postponeAfter.ToLogString()}.", Activity, cancellationToken);
            throw new OperationCanceledException("This exception will eventually be converted to a RequestPostponed exception.");
        }

        // We will postpone if the reduced total execution time has passed
        // This should normally not happen, if the postpone limit above has been set to a low enough value
        Activity.ActivityInformation.Workflow.ReducedTimeCancellationToken.ThrowIfCancellationRequested();
        return await methodAsync(cancellationToken);

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
