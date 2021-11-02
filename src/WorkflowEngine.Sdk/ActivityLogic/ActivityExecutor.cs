using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;

namespace Nexus.Link.WorkflowEngine.Sdk.ActivityLogic
{
    public class ActivityExecutor
    {
        public IWorkflowVersion WorkflowVersion { get; }
        public Activity Activity { get; set; }

        public ActivityInstance ActivityInstance => Activity.Instance;

        public ActivityVersion ActivityVersion => Activity.Version;

        public ActivityExecutor(IWorkflowVersion workflowVersion, Activity activity)
        {
            InternalContract.RequireNotNull(workflowVersion, nameof(workflowVersion));
            InternalContract.RequireNotNull(activity, nameof(activity));
            WorkflowVersion = workflowVersion;
            Activity = activity;
        }

        public async Task ExecuteAsync(ActivityMethod method, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));

            await SafeExecuteAsync(
                async (a, ct) =>
                {
                    await method(a, ct);
                    return true;
                },
                true,
                ct => Task.FromResult(false),
                cancellationToken);
        }

        public Task<TMethodReturnType> ExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));

            return SafeExecuteAsync(method, false, getDefaultValueMethodAsync, cancellationToken);
        }

        private async Task<TMethodReturnType> SafeExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            bool ignoreReturnValue,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            try
            {
                // Already have a result?
                FulcrumAssert.IsNotNull(Activity.Instance, CodeLocation.AsString());
                FulcrumAssert.IsNotNullOrWhiteSpace(Activity.Instance.Id);
                Activity.WorkflowCache.AddActivity(Activity.Instance.Id, Activity);

                if (Activity.Instance.HasCompleted)
                {
                    return await SafeGetResultOrThrowAsync(ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
                }

                Activity.WorkflowCache.LatestActivityInstanceId = Activity.Instance.Id;

                if (!string.IsNullOrWhiteSpace(Activity.Instance.AsyncRequestId))
                {
                    throw new HandledRequestPostponedException(Activity.Instance.AsyncRequestId);
                }

                await SafeVerifyMaxTimeAsync();

                await SafeCallMethodAndUpdateActivityInformationAsync(method, ignoreReturnValue, cancellationToken);

                return await SafeGetResultOrThrowAsync(ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
            }
            catch (HandledRequestPostponedException)
            {
                throw;
            }
            catch (FulcrumCancelledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new FulcrumAssertionFailedException($"{CodeLocation.AsString()}: Unexpected exception: {e} detected", e);
            }
        }

        private async Task<TMethodReturnType> SafeGetResultOrThrowAsync<TMethodReturnType>(
            bool ignoreReturnValue, Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            if (Activity.Instance.State != ActivityStateEnum.Failed)
            {
                return ignoreReturnValue
                    ? default
                    : JsonHelper.SafeDeserializeObject<TMethodReturnType>(Activity.Instance.ResultAsJson);
            }

            switch (Activity.Version.FailUrgency)
            {
                case ActivityFailUrgencyEnum.CancelWorkflow:
                    throw new FulcrumCancelledException($"Activity {Activity} failed with the following message: {Activity.Instance.ExceptionTechnicalMessage}");
                case ActivityFailUrgencyEnum.Stopping:
                    throw new HandledRequestPostponedException();
                default:
                    if (getDefaultValueMethodAsync == null) return default;
                    try
                    {
                        return await getDefaultValueMethodAsync(cancellationToken);
                    }
                    catch (Exception)
                    {
                        // Errors in the default method overrides stopping.
                        // TODO: How do we convey information about this to the person who has to deal with this stopping activity?
                        // TODO: Log
                        throw new HandledRequestPostponedException
                        {
                            TryAgain = true
                        };
                    }
            }
        }

        private async Task SafeCallMethodAndUpdateActivityInformationAsync<TMethodReturnType>(ActivityMethod<TMethodReturnType> method, bool ignoreReturnValue, CancellationToken cancellationToken)
        {
            // Call the activity. The method will only return if this is a method with no external calls.
            var activityInstance = Activity.Instance;
            try
            {
                if (ignoreReturnValue)
                {
                    await method(Activity, cancellationToken);
                    activityInstance.ResultAsJson = "";
                }
                else
                {
                    var result = await method(Activity, cancellationToken);
                    activityInstance.ResultAsJson = result.ToJsonString();
                }
                activityInstance.State = ActivityStateEnum.Success;
                activityInstance.FinishedAt = DateTimeOffset.UtcNow;
            }
            catch (FulcrumCancelledException)
            {
                throw;
            }
            catch (HandledRequestPostponedException)
            {
                throw;
            }
            catch (ActivityPostponedException)
            {
                throw new HandledRequestPostponedException();
            }
            catch (FulcrumTryAgainException)
            {
                activityInstance.State = ActivityStateEnum.Waiting;
                await SafeAlertExceptionAsync(cancellationToken);
                throw new HandledRequestPostponedException
                {
                    TryAgain = true
                };
            }
            catch (RequestPostponedException e)
            {
                if (e.WaitingForRequestIds == null || e.WaitingForRequestIds.Count != 1) throw;
                activityInstance.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
                activityInstance.State = ActivityStateEnum.Waiting;
                await SafeAlertExceptionAsync(cancellationToken);
                throw new HandledRequestPostponedException(e);
            }
            catch (Exception e)
            {
                // Normal error
                // TODO: Handle error: Send event, throw postpone if halt
                activityInstance.State = ActivityStateEnum.Failed;
                activityInstance.FinishedAt = DateTimeOffset.UtcNow;
                activityInstance.ExceptionCategory = ActivityExceptionCategoryEnum.Technical;
                activityInstance.ExceptionTechnicalMessage =
                    $"A local method throw an exception of type {e.GetType().FullName} and message: {e.Message}";
                activityInstance.ExceptionFriendlyMessage =
                    $"A local method failed with the following message: {e.Message}";
            }

            await SafeAlertExceptionAsync(cancellationToken);
        }

        private async Task SafeAlertExceptionAsync(CancellationToken cancellationToken)
        {
            if (Activity.Instance.State != ActivityStateEnum.Failed) return;
            if (Activity.Instance.ExceptionAlertHandled.HasValue &&
                Activity.Instance.ExceptionAlertHandled.Value) return;

            if (!(WorkflowVersion is IActivityExceptionAlertHandler alertHandler)) return;

            try
            {
                FulcrumAssert.IsNotNull(Activity.Instance.ExceptionCategory, CodeLocation.AsString());
                var alert = new ActivityExceptionAlert
                {
                    WorkflowInstanceId = Activity.WorkflowCache.Instance.Id,
                    ActivityInstanceId = Activity.Instance.Id,
                    ExceptionCategory = Activity.Instance.ExceptionCategory!.Value,
                    ExceptionFriendlyMessage =
                        Activity.Instance.ExceptionFriendlyMessage,
                    ExceptionTechnicalMessage =
                        Activity.Instance.ExceptionTechnicalMessage
                };
                try
                {
                    var handled =
                        await alertHandler.HandleActivityExceptionAlertAsync(alert, cancellationToken);
                    Activity.Instance.ExceptionAlertHandled = handled;
                }
                catch (Exception)
                {
                    // We will try again next reentry.
                }
            }
            catch (Exception)
            {
                // TODO: Log
                throw new HandledRequestPostponedException(Activity.Instance.AsyncRequestId)
                {
                    TryAgain = true
                };
            }
        }

        private static Task SafeVerifyMaxTimeAsync()
        {
            return Task.CompletedTask;
        }
    }
}