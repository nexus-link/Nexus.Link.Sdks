using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class LogicExecutor : ILogicExecutor
    {
        public IInternalActivity Activity { get; }

        public LogicExecutor(IInternalActivity activity)
        {
            Activity = activity;
            InternalContract.RequireNotNull(activity, nameof(activity));
        }

        /// <inheritdoc />
        public async Task ExecuteWithoutReturnValueAsync(InternalActivityMethodAsync methodAsync, string methodName, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            InternalContract.RequireNotNull(methodName, nameof(methodName));
            var (_, exception) = await SafeExecuteAsync(async ct =>
                {
                    await methodAsync(ct);
                    return Task.FromResult(true);
                }, methodName, false, cancellationToken);
            if (exception != null) throw exception;
        }

        /// <inheritdoc />
        public async Task<TMethodReturns> ExecuteWithReturnValueAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            InternalContract.RequireNotNull(methodName, nameof(methodName));
            var (result, exception) = await SafeExecuteAsync(methodAsync, methodName, true, cancellationToken);
            if (exception != null) throw exception;
            return result;
        }

        /// <summary>
        /// The single purpose of this method is to verify that we limit the type of exceptions that can be thrown.
        /// </summary>
        /// <exception cref="ActivityFailedException"></exception>
        /// <exception cref="RequestPostponedException"></exception>
        /// <exception cref="WorkflowImplementationShouldNotCatchThisException"></exception>
        /// <exception cref="IgnoreAndExitToParentException"></exception>
        private async Task<(TMethodReturns, Exception)> SafeExecuteAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
            bool hasReturnValue,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            Exception exception;
            try
            {
                (var result, exception) =
                    await LogAndExecuteAsync(methodAsync, methodName, hasReturnValue, cancellationToken);
                if (exception == null) return (result, null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            switch (exception)
            {
                case ActivityFailedException
                    or RetryActivityFromCatchException
                    or RequestPostponedException
                    or WorkflowFastForwardBreakException
                    or WorkflowImplementationShouldNotCatchThisException
#pragma warning disable CS0618
                    or IgnoreAndExitToParentException:
                    return (default, exception);
                default:
                    // Unexpected exception. Our conclusion is that there is an error in the workflow engine.
                    var unexpectedException = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                        $"The workflow engine threw an unexpected exception when executing a activity's logic method ({methodName}). This is the exception:\r{exception}",
                        $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
                    return (default, unexpectedException);
            }
        }

        private async Task<(TMethodReturns, Exception)> LogAndExecuteAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
            bool hasReturnValue,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await Activity.LogInformationAsync($"Activity {Activity.ToLogString()} method {methodName} started.", new { Name = methodName }, cancellationToken);

            var (result, exception) = await ExecuteAndConsolidateExceptionsAsync(methodAsync, cancellationToken);
            stopwatch.Stop();
            if (exception is WorkflowImplementationShouldNotCatchThisException ex)
            {
                await Activity.LogInformationAsync($"Activity {Activity.ToLogString()} child threw",
                    new
                    {
                        ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"),
                        Exception = $"{ex.InnerException.GetType().Name}: {ex.InnerException.Message}",
                    }, cancellationToken);
                return (result, exception);
            }
            if (exception != null)
            {
                await Activity.LogInformationAsync($"Activity {Activity.ToLogString()} method {methodName} threw",
                    new
                    {
                        ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"),
                        Exception = $"{exception.GetType().Name}: {exception.Message}",
                    }, cancellationToken);
                return (result, exception);
            }
            object data;
            if (hasReturnValue)
            {
                data = new
                {
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"),
                    Result = result
                };
            }
            else
            {
                data = new { ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2") };
            }
            await Activity.LogInformationAsync($"Activity {Activity.ToLogString()} method {methodName} returned.", data, cancellationToken);

            return (result, null);
        }

        private async Task<(TMethodReturns, Exception)> ExecuteAndConsolidateExceptionsAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, CancellationToken cancellationToken)
        {
            try
            {
                // We will postpone if the reduced total execution time has passed
                // This should normally not happen, if the workflow postpone limit has been set to a low enough value
                Activity.ActivityInformation.Workflow.ReducedTimeCancellationToken.ThrowIfCancellationRequested();
                var result = await methodAsync(Activity.ActivityInformation.Workflow.ReducedTimeCancellationToken);
                return (result, null);
            }
            catch (ActivityFailedException ex) // Also covers WorkflowFailedException
            {
                return (default, ex);
            }
            catch (RetryActivityFromCatchException ex)
            {
                return (default, ex);
            }
            catch (RequestPostponedException ex)
            {
                return (default, ex);
            }
#pragma warning disable CS0618
            catch (ActivityException ex)
#pragma warning disable CS0618
            {
                var exception = new ActivityFailedException(ex.ExceptionCategory, ex.TechnicalMessage, ex.FriendlyMessage);
                return (default, exception);
            }
            catch (WorkflowImplementationShouldNotCatchThisException ex)
            {
                FulcrumAssert.IsNotNull(ex.InnerException, CodeLocation.AsString());
                // TODO: Experiment
                //return (default, ex.InnerException);
                return (default, ex);
            }
            catch (FulcrumException e)
            {
                return (default, ConvertFulcrumException(e));
            }
            catch (OperationCanceledException)
            {
                var exception = new ActivityPostponedException(TimeSpan.Zero);
                return (default, exception);
            }
            catch (Exception e)
            {
                // Unexpected exception. Our conclusion is that the workflow implementer has made a programmers error.
                var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowImplementationError,
                    $"An activity method threw an unexpected exception {e.GetType().Name}. The workflow programmer is expected to catch this exception and throw an {nameof(ActivityFailedException)}. More information about the exception:\r{e}",
                    $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
                return (default, exception);
            }
        }

        private static Exception ConvertFulcrumException(FulcrumException e)
        {
            if (e.IsRetryMeaningful)
            {
                var timeSpan = TimeSpan.FromSeconds(e.RecommendedWaitTimeInSeconds);
                if (timeSpan < TimeSpan.FromSeconds(1))
                {
                    timeSpan = TimeSpan.FromSeconds(1);
                }

                return new ActivityPostponedException(timeSpan);
            }

            if (e is FulcrumProgrammersErrorException)
            {
                // The workflow implementor has made a programmers error
                return new ActivityFailedException(
                    ActivityExceptionCategoryEnum.WorkflowImplementationError,
                    $"The activity threw an exception that indicates a programmers error:\r{e}",
                    "The workflow implementation encountered an unexpected error. Please contact the workflow developer.");
            }

            return new ActivityFailedException(
                e is FulcrumBusinessRuleException
                    ? ActivityExceptionCategoryEnum.BusinessError
                    : ActivityExceptionCategoryEnum.TechnicalError,
                $"The activity threw a permanent exception: {e}",
                $"The activity failed with the following message: {e.FriendlyMessage}");
        }
    }
}