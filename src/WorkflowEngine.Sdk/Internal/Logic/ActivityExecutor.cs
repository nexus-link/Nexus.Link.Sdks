using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityExecutor : IActivityExecutor
    {
        public IInternalActivity Activity { get; }

        public ActivityExecutor(IInternalActivity activity)
        {
            Activity = activity;
            InternalContract.RequireNotNull(activity, nameof(activity));
        }

        public async Task ExecuteWithoutReturnValueAsync(InternalActivityMethodAsync methodAsync, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

            var task = ExecuteAsync(async ct => { await methodAsync(ct); return true; }, false, cancellationToken);
            await ProtectExceptionsFromGettingCaughtAsync(task);
        }

        public async Task<TMethodReturns> ExecuteWithReturnValueAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync,
            ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));

            var task = ExecuteAsync(methodAsync, true, cancellationToken);
            await ProtectExceptionsFromGettingCaughtAsync(task);
            TMethodReturns result;
            if (Activity.Instance.State == ActivityStateEnum.Success)
            {
                result = JsonHelper.SafeDeserializeObject<TMethodReturns>(Activity.Instance.ResultAsJson);
            }
            else
            {
                // We will typically get here if the activity failed and the activity was configured to ignore the failure
                result = await GetDefaultValueAsync(getDefaultValueAsync, cancellationToken);
            }
            return result;
        }

        private async Task ExecuteAsync<TMethodReturns>(InternalActivityMethodAsync<TMethodReturns> methodAsync,
            bool hasReturnValue,
            CancellationToken cancellationToken)
        {
            await Activity.LogInformationAsync($"Begin activity {Activity}", Activity.Instance, cancellationToken);
            try
            {
                if (Activity.Instance.HasCompleted) return;
                await CallMethodAndUpdateActivityInformationAsync(methodAsync, hasReturnValue, cancellationToken);
                FulcrumAssert.IsTrue(Activity.Instance.HasCompleted, CodeLocation.AsString());
                await MaybeThrowAsync(cancellationToken);
            }
            finally
            {
                Activity.MaybePurgeLogs();
                await Activity.LogInformationAsync($"End activity {Activity}", Activity.Instance, cancellationToken);
            }
        }

        private async Task CallMethodAndUpdateActivityInformationAsync<TMethodReturns>(
            InternalActivityMethodAsync<TMethodReturns> methodAsync, bool hasReturnValue, CancellationToken cancellationToken)
        {
            try
            {
                if (Activity.ActivityInformation.Options.ActivityMaxExecutionTimeSpan.HasValue)
                {
                    var maxExecutionTime = Activity.ActivityInformation.Options.ActivityMaxExecutionTimeSpan.Value;
                    var startedAt = Activity.ActivityStartedAt;
                    var expirationTime = startedAt.Add(maxExecutionTime);
                    var now = DateTimeOffset.UtcNow;
                    if (expirationTime <= now)
                    {
                        throw new ActivityFailedException(ActivityExceptionCategoryEnum.MaxTimeReachedError,
                            $"The maximum time for the activity has been reached. The activity was started at {startedAt.ToLogString()} and expired at {expirationTime.ToLogString()}, it is now {now.ToLogString()}",
                            "The maximum time for the activity has been reached.");
                    }
                }
                await CallMethodAndLogAsync(methodAsync, hasReturnValue, cancellationToken);
                Activity.MarkAsSuccess();
            }
            catch (WorkflowImplementationShouldNotCatchThisException)
            {
                throw;
            }
            catch (WorkflowFailedException e)
            {
                Activity.MarkAsFailed(e);
                await Activity.SafeAlertExceptionAsync(cancellationToken);
                throw;
            }
            catch (ActivityFailedException e)
            {
                Activity.MarkAsFailed(e);
                await Activity.SafeAlertExceptionAsync(cancellationToken);
            }
#pragma warning disable CS0618
            catch (ActivityException e)
            {
                Activity.MarkAsFailed(e);
                await Activity.SafeAlertExceptionAsync(cancellationToken);
            }
#pragma warning restore CS0618
            catch (ActivityPostponedException)
            {
                Activity.Instance.State = ActivityStateEnum.Waiting;
                throw new RequestPostponedException();
            }
            catch (FulcrumException e)
            {
                ActivityFailedException exception;
                if (e.IsRetryMeaningful)
                {
                    Activity.Instance.State = ActivityStateEnum.Waiting;
                    var timeSpan = TimeSpan.FromSeconds(e.RecommendedWaitTimeInSeconds);
                    if (timeSpan < TimeSpan.FromSeconds(1))
                    {
                        timeSpan = TimeSpan.FromSeconds(60);
                    }
                    throw new RequestPostponedException
                    {
                        TryAgain = e.IsRetryMeaningful,
                        TryAgainAfterMinimumTimeSpan = timeSpan
                    };
                }
                
                if (e is FulcrumProgrammersErrorException)
                {
                    // The workflow implementor has made a programmers error
                    exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowImplementationError,
                        $"The activity threw an exception that indicates a programmers error:\r{e}",
                        "The workflow implementation encountered an unexpected error. Please contact the workflow developer.");
                }
                else
                {
                    exception = new ActivityFailedException(
                        e is FulcrumBusinessRuleException
                            ? ActivityExceptionCategoryEnum.BusinessError
                            : ActivityExceptionCategoryEnum.TechnicalError,
                        $"The activity throw an exception: {e}",
                        $"The activity failed with the following message: {e.FriendlyMessage}");
                }
                Activity.MarkAsFailed(exception);
                await Activity.SafeAlertExceptionAsync(cancellationToken);
            }
            catch (RequestPostponedException e)
            {
                Activity.Instance.State = ActivityStateEnum.Waiting;
                if (e.WaitingForRequestIds.Count == 1)
                {
                    Activity.Instance.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
                }
                throw;
            }
            catch (Exception e)
            {
                // Unexpected exception, seen as the workflow implementer has made a programmers error.
                var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowImplementationError,
                            $" The activity threw the following unexpected exception, which is considered a workflow implementation error:\r{e}",
                            "The workflow implementation encountered an unexpected error. Please contact the workflow developer.");
                Activity.MarkAsFailed(exception);
                await Activity.SafeAlertExceptionAsync(cancellationToken);
            }
        }

        private async Task CallMethodAndLogAsync<TMethodReturns>(InternalActivityMethodAsync<TMethodReturns> methodAsync, bool hasReturnValue,
            CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(methodAsync, nameof(methodAsync));
            await Activity.LogVerboseAsync($"Begin activity {Activity} method.", null, cancellationToken);
            TMethodReturns result = default;
            var hasResult = false;
            try
            {
                result = await methodAsync(cancellationToken);
                if (hasReturnValue) {
                    hasResult = true;
                    Activity.Instance.ResultAsJson = result.ToJsonString();
                }
            }
            catch (Exception e) when (
                e is WorkflowImplementationShouldNotCatchThisException
                    or ActivityPostponedException
                    or RequestPostponedException
                    or FulcrumException)
            {
                await Activity.LogInformationAsync($"Activity {Activity} method threw exception: {e}", e, cancellationToken);
                throw;
            }
            catch (Exception e)
            {
                await Activity.LogWarningAsync($"Activity {Activity} method threw exception: {e}", e, cancellationToken);
                throw;
            }
            finally
            {
                await Activity.LogVerboseAsync($"End activity {Activity} method.", hasResult ? result : null, cancellationToken);
            }
        }

        private async Task MaybeThrowAsync(CancellationToken cancellationToken)
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

        private async Task<TMethodReturns> GetDefaultValueAsync<TMethodReturns>(ActivityDefaultValueMethodAsync<TMethodReturns> getDefaultValueAsync, CancellationToken cancellationToken)
        {
            if (getDefaultValueAsync == null) return default;
            try
            {
                return await getDefaultValueAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await Activity.LogErrorAsync(
                    $"The default value method for activity {Activity} threw an exception." +
                    $" The default value for {typeof(TMethodReturns).Name} ({default(TMethodReturns)}) is used instead.",
                    e, cancellationToken);
                return default;
            }
        }

        private async Task ProtectExceptionsFromGettingCaughtAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (WorkflowImplementationShouldNotCatchThisException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new WorkflowImplementationShouldNotCatchThisException(e);
            }
        }
    }
}