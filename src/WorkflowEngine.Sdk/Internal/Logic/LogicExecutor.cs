using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

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
            await SafeExecuteAsync(async ct =>
                {
                    await methodAsync(ct);
                    return Task.FromResult(true);
                }, methodName, true, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TMethodReturns> ExecuteWithReturnValueAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            InternalContract.RequireNotNull(methodName, nameof(methodName));
            var result = await SafeExecuteAsync(methodAsync, methodName, true, cancellationToken);
            return result;
        }

        /// <summary>
        /// The single purpose of this method is to verify that we limit the type of exceptions that can be thrown.
        /// </summary>
        /// <exception cref="ActivityFailedException"></exception>
        /// <exception cref="RequestPostponedException"></exception>
        /// <exception cref="WorkflowImplementationShouldNotCatchThisException"></exception>
        /// <exception cref="IgnoreAndExitToParentException"></exception>
        private async Task<TMethodReturns> SafeExecuteAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
            bool hasReturnValue,
            CancellationToken cancellationToken = default)
        {
            try
            {
                InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
                var result = await LogAndExecuteAsync(methodAsync, methodName, hasReturnValue, cancellationToken);
                return result;
            }
            catch (Exception e)
                when (e is ActivityFailedException
                          or RetryActivityFromCatchException
                          or RequestPostponedException
                          or WorkflowFastForwardBreakException
                          or WorkflowImplementationShouldNotCatchThisException
#pragma warning disable CS0618
                          or IgnoreAndExitToParentException)
#pragma warning restore CS0618
            {
                throw;
            }
            catch (Exception e)
            {
                // Unexpected exception. Our conclusion is that there is an error in the workflow engine.
                var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError,
                    $"The workflow engine threw an unexpected exception when executing a activity's logic method ({methodName}). This is the exception:\r{e}",
                    $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
                throw exception;
            }
        }

        private async Task<TMethodReturns> LogAndExecuteAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, string methodName,
            bool hasReturnValue,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await Activity.LogInformationAsync($"Activity {Activity.ToLogString()} method {methodName} started.", new { Name = methodName }, cancellationToken);
            try
            {
                var result = await ExecuteAndConsolidateExceptionsAsync(methodAsync, cancellationToken);
                stopwatch.Stop();
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

                return result;
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                await Activity.LogInformationAsync($"Activity {Activity.ToLogString()} method {methodName} threw",
                    new
                    {
                        ElapsedSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2"),
                        Exception = $"{e.GetType().Name}: {e.Message}",
                    }, cancellationToken);
                throw;
            }
        }

        private async Task<TMethodReturns> ExecuteAndConsolidateExceptionsAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, CancellationToken cancellationToken)
        {
            try
            {
                // We will postpone if the reduced total execution time has passed
                // This should normally not happen, if the workflow postpone limit has been set to a low enough value
                Activity.ActivityInformation.Workflow.ReducedTimeCancellationToken.ThrowIfCancellationRequested();
                return await methodAsync(Activity.ActivityInformation.Workflow.ReducedTimeCancellationToken);
            }
            catch (ActivityFailedException) // Also covers WorkflowFailedException
            {
                throw;
            }
            catch (RetryActivityFromCatchException)
            {
                throw;
            }
            catch (RequestPostponedException)
            {
                throw;
            }
#pragma warning disable CS0618
            catch (ActivityException e)
#pragma warning disable CS0618
            {
                throw new ActivityFailedException(e.ExceptionCategory, e.TechnicalMessage, e.FriendlyMessage);
            }
            catch (WorkflowImplementationShouldNotCatchThisException e)
            {
                FulcrumAssert.IsNotNull(e.InnerException, CodeLocation.AsString());
                throw e.InnerException!;
            }
            catch (FulcrumException e)
            {
                throw ConvertFulcrumException(e);
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
                // Unexpected exception. Our conclusion is that the workflow implementer has made a programmers error.
                var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowImplementationError,
                    $"An activity method threw an unexpected exception {e.GetType().Name}. The workflow programmer is expected to catch this exception and throw an {nameof(ActivityFailedException)}. More information about the exception:\r{e}",
                    $"The workflow \"{Activity.ActivityInformation.Workflow.InstanceTitle}\" encountered an unexpected error. Please contact the workflow developer.");
                throw exception;
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

                return new RequestPostponedException
                {
                    TryAgain = true,
                    TryAgainAfterMinimumTimeSpan = timeSpan
                };
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