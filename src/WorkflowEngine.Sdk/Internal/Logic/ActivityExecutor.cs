using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityExecutor : IActivityExecutor
    {
        public Activity Activity { get; }

        public ActivityExecutor(Activity activity)
        {
            Activity = activity;
            InternalContract.RequireNotNull(activity, nameof(activity));
        }

        public async Task ExecuteWithoutReturnValueAsync(ActivityMethod method, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));

            var task = ExecuteAsync(async ct => { await method(ct); return true; }, false, cancellationToken);
            await ProtectExceptionsFromGettingCaughtAsync(task);
        }

        public async Task<TMethodResult> ExecuteWithReturnValueAsync<TMethodResult>(
            ActivityMethod<TMethodResult> method,
            Func<CancellationToken, Task<TMethodResult>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));

            var task = ExecuteAsync(method, true, cancellationToken);
            await ProtectExceptionsFromGettingCaughtAsync(task);
            TMethodResult result;
            if (Activity.Instance.State == ActivityStateEnum.Success)
            {
                result = JsonHelper.SafeDeserializeObject<TMethodResult>(Activity.Instance.ResultAsJson);
            }
            else
            {
                // We will typically get here if the activity failed and the activity was configured to ignore the failure
                result = await GetDefaultValueAsync(getDefaultValueMethodAsync, cancellationToken);
            }
            return result;
        }

        private async Task ExecuteAsync<TMethodResult>(ActivityMethod<TMethodResult> method,
            bool hasReturnValue,
            CancellationToken cancellationToken)
        {
            await Activity.LogInformationAsync($"Begin activity {Activity}", Activity.Instance, cancellationToken);
            try
            {
                Activity.ActivityInformation.Workflow.AddActivity(Activity);
                Activity.ActivityInformation.Workflow.LatestActivity = Activity;
                WorkflowStatic.Context.LatestActivity = Activity;
                if (Activity.Instance.HasCompleted) return;
                await CallMethodAndUpdateActivityInformationAsync(method, hasReturnValue, cancellationToken);
                FulcrumAssert.IsTrue(Activity.Instance.HasCompleted, CodeLocation.AsString());
                await MaybeThrowAsync(cancellationToken);
            }
            finally
            {
                Activity.MaybePurgeLogs(cancellationToken);
                await Activity.LogInformationAsync($"End activity {Activity}", Activity.Instance, cancellationToken);
            }
        }

        private async Task CallMethodAndUpdateActivityInformationAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method, bool hasReturnValue, CancellationToken cancellationToken)
        {
            // Call the Activity. The method will only return if this is a method with no external calls.
            var activityInstance = Activity.Instance;
            try
            {
                await CallMethodAndLogAsync(method, hasReturnValue, cancellationToken, activityInstance);
                activityInstance.State = ActivityStateEnum.Success;
                activityInstance.FinishedAt = DateTimeOffset.UtcNow;
                activityInstance.ContextDictionary.Clear();
            }
            catch (WorkflowImplementationShouldNotCatchThisException)
            {
                throw;
            }
            catch (WorkflowFailedException e)
            {
                await Activity.MarkasFailedAsync(e, cancellationToken);
                throw;
            }
            catch (ActivityFailedException e)
            {
                await Activity.MarkasFailedAsync(e, cancellationToken);
            }
#pragma warning disable CS0618
            catch (ActivityException e)
            {
                await Activity.MarkasFailedAsync(e, cancellationToken);
            }
#pragma warning restore CS0618
            catch (ActivityPostponedException)
            {
                Activity.Instance.State = ActivityStateEnum.Waiting;
                throw new RequestPostponedException();
            }
            catch (FulcrumTryAgainException e)
            {
                Activity.Instance.State = ActivityStateEnum.Waiting;
                throw new RequestPostponedException
                {
                    TryAgain = true,
                    TryAgainAfterMinimumTimeSpan = TimeSpan.FromSeconds(e.RecommendedWaitTimeInSeconds)
                };
            }
            catch (RequestPostponedException e)
            {
                Activity.Instance.State = ActivityStateEnum.Waiting;
                if (e.WaitingForRequestIds.Count == 1)
                {
                    activityInstance.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
                }
                throw;
            }
            catch (Exception e)
            {
                // Unexpected exception
                var exception = new ActivityFailedException(ActivityExceptionCategoryEnum.WorkflowImplementationError,
                            $"An activity is only supposed to throw a limited set of exceptions."
                            + $" The activity threw the following unexpected exception:\r{e}",
                            "The workflow implementation encountered an unexpected error. Please contact the workflow developer.");
                await Activity.MarkasFailedAsync(exception, cancellationToken);
            }
        }

        private async Task CallMethodAndLogAsync<TMethodReturnType>(ActivityMethod<TMethodReturnType> method, bool hasReturnValue,
            CancellationToken cancellationToken, ActivityInstance activityInstance)
        {
            TMethodReturnType result = default;
            var hasResult = false;
            try
            {
                await Activity.LogVerboseAsync($"Begin activity {Activity} method.", null, cancellationToken);
                result = await method(cancellationToken);
                if (hasReturnValue) {
                    hasResult = true;
                    activityInstance.ResultAsJson = result.ToJsonString();
                }
            }
            catch (Exception e) when (
                e is WorkflowImplementationShouldNotCatchThisException
                    or ActivityPostponedException
                    or RequestPostponedException
                    or FulcrumTryAgainException)
            {
                await Activity.LogInformationAsync($"Activity {Activity} method threw exception {e.GetType().Name}.", e, cancellationToken);
                throw;
            }
            catch (Exception e)
            {
                await Activity.LogWarningAsync($"Activity {Activity} method threw exception {e.GetType().Name}: {e.Message}", e, cancellationToken);
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
            await Activity.SafeAlertExceptionAsync(cancellationToken);
            switch (Activity.Version.FailUrgency)
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

        private async Task<TMethodResult> GetDefaultValueAsync<TMethodResult>(Func<CancellationToken, Task<TMethodResult>> getDefaultValueMethodAsync, CancellationToken cancellationToken)
        {
            if (getDefaultValueMethodAsync == null) return default;
            try
            {
                return await getDefaultValueMethodAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await Activity.LogErrorAsync(
                    $"The default value method for activity {Activity} threw an exception." +
                    $" The default value for {typeof(TMethodResult).Name} ({default(TMethodResult)}) is used instead.",
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