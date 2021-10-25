using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public class ActivityExecutor : IActivityExecutor
    {
        public IWorkflowVersionBase WorkflowVersion { get; }
        public Activity Activity { get; set; }
        public IAsyncRequestClient AsyncRequestClient { get; }

        public ActivityExecutor(IWorkflowVersionBase workflowVersion, IAsyncRequestClient asyncRequestClient)
        {
            WorkflowVersion = workflowVersion;
            AsyncRequestClient = asyncRequestClient;
        }

        private ActivityInformation ActivityInformation => Activity.ActivityInformation;

        public Task<TMethodReturnType> ExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.Require(Activity != null, $"Property {nameof(Activity)} must be set before calling the {nameof(ExecuteAsync)} method.");

            return InternalExecuteAsync(method, false, getDefaultValueMethodAsync, cancellationToken);
        }

        public async Task ExecuteAsync(
            ActivityMethod method,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.Require(Activity != null, $"Property {nameof(Activity)} must be set before calling the {nameof(ExecuteAsync)} method.");

            await InternalExecuteAsync(async (instance, ct) =>
            {
                await method(instance, ct);
                return Task.FromResult(false);
            }, true, null, cancellationToken);
        }

        private async Task<TMethodReturnType> InternalExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            bool ignoreReturnValue,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.Require(Activity != null, $"Property {nameof(Activity)} must be set before calling the {nameof(InternalExecuteAsync)} method.");

            await SafeSaveActivityInformationAsync(true, cancellationToken);

            // Already have a result?
            if (ActivityInformation.HasCompleted)
            {
                return await SafeGetResultOrThrowAsync(false, ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(ActivityInformation.Activity.Instance.AsyncRequestId))
            {
                throw new HandledRequestPostponedException(ActivityInformation.Activity.Instance.AsyncRequestId);
            }

            await SafeVerifyMaxTimeAsync();

            await SafeCallMethodAndUpdateActivityInformationAsync(method, ignoreReturnValue, cancellationToken);

            return await SafeGetResultOrThrowAsync(false, ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
        }

        private async Task SafeUpdateInstanceWithResultAndAlertExceptionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (ActivityInformation.Activity.Instance.State == ActivityStateEnum.Failed)
                {


                    if (!ActivityInformation.Activity.Instance.ExceptionAlertHandled.HasValue ||
                        !ActivityInformation.Activity.Instance.ExceptionAlertHandled.Value)
                    {
                        if (WorkflowVersion is IActivityExceptionAlertHandler alertHandler)
                        {
                            FulcrumAssert.IsNotNull(ActivityInformation.Activity.Instance.ExceptionCategory,
                                CodeLocation.AsString());
                            var alert = new ActivityExceptionAlert
                            {
                                WorkflowInstanceId = ActivityInformation.WorkflowInformation.InstanceId,
                                ActivityInstanceId = ActivityInformation.Activity.Instance.Id,
                                ExceptionCategory = ActivityInformation.Activity.Instance.ExceptionCategory!.Value,
                                ExceptionFriendlyMessage = ActivityInformation.Activity.Instance.ExceptionFriendlyMessage,
                                ExceptionTechnicalMessage = ActivityInformation.Activity.Instance.ExceptionTechnicalMessage
                            };
                            try
                            {
                                var handled =
                                    await alertHandler.HandleActivityExceptionAlertAsync(alert, cancellationToken);
                                ActivityInformation.Activity.Instance.ExceptionAlertHandled = handled;
                            }
                            catch (Exception)
                            {
                                // We will try again next reentry.
                            }
                        }
                    }
                }
                await SafeSaveActivityInformationAsync(false, cancellationToken);
            }
            catch (Exception)
            {
                // TODO: Log
                throw new HandledRequestPostponedException(ActivityInformation.Activity.Instance.AsyncRequestId)
                {
                    TryAgain = true
                };
            }
        }

        private async Task<TMethodReturnType> SafeGetResultOrThrowAsync<TMethodReturnType>(bool publishEvent,
            bool ignoreReturnValue, Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            if (ActivityInformation.Activity.Instance.State != ActivityStateEnum.Failed)
            {
                return ignoreReturnValue
                    ? default
                    : JsonHelper.SafeDeserializeObject<TMethodReturnType>(ActivityInformation.Activity.Instance.ResultAsJson);
            }

            if (publishEvent)
            {
                // Publish message about exception
            }

            switch (ActivityInformation.Activity.Version.FailUrgency)
            {
                case ActivityFailUrgencyEnum.CancelWorkflow:
                    throw new FulcrumCancelledException($"Activity {Activity} failed with the following message: {ActivityInformation.Activity.Instance.ExceptionTechnicalMessage}");
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
            try
            {
                if (ignoreReturnValue)
                {
                    await method(Activity, cancellationToken);
                    ActivityInformation.Activity.Instance.ResultAsJson = "";
                }
                else
                {
                    var result = await method(Activity, cancellationToken);
                    ActivityInformation.Activity.Instance.ResultAsJson = result.ToJsonString();
                    ActivityInformation.Activity.Instance.State = ActivityStateEnum.Success;
                }
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
                ActivityInformation.Activity.Instance.State = ActivityStateEnum.Waiting;
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
                throw new HandledRequestPostponedException
                {
                    TryAgain = true
                };
            }
            catch (RequestPostponedException e)
            {
                if (e.WaitingForRequestIds == null || e.WaitingForRequestIds.Count != 1) throw;
                ActivityInformation.Activity.Instance.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
                ActivityInformation.Activity.Instance.State = ActivityStateEnum.Waiting;
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
                throw new HandledRequestPostponedException(e);
            }
            catch (Exception e)
            {
                // Normal error
                // TODO: Handle error: Send event, throw postpone if halt
                ActivityInformation.Activity.Instance.State = ActivityStateEnum.Failed;
                ActivityInformation.Activity.Instance.ExceptionCategory = ActivityExceptionCategoryEnum.Technical;
                ActivityInformation.Activity.Instance.ExceptionTechnicalMessage =
                    $"A local method throw an exception of type {e.GetType().FullName} and message: {e.Message}";
                ActivityInformation.Activity.Instance.ExceptionFriendlyMessage =
                    $"A local method failed with the following message: {e.Message}";
            }

            if (ActivityInformation.Activity.Instance.Id != null)
            {
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
            }
        }

        private static Task SafeVerifyMaxTimeAsync()
        {
            return Task.CompletedTask;
        }

        private async Task SafeSaveActivityInformationAsync(bool newActivity, CancellationToken cancellationToken)
        {
            try
            {
                await ActivityInformation.PersistAsync(cancellationToken);
                FulcrumAssert.IsNotNull(ActivityInformation.Activity.Instance.Id, CodeLocation.AsString());
                if (newActivity) ActivityInformation.WorkflowInformation.AddActivity(ActivityInformation.Activity.Instance.Id, Activity);
            }
            catch (Exception)
            {
                // Save failed
                // TODO: Log
                throw new HandledRequestPostponedException(ActivityInformation.Activity.Instance.AsyncRequestId)
                {
                    TryAgain = true
                };
            }
        }
    }
}