using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;

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

        private ActivityPersistence ActivityPersistence => Activity.ActivityPersistence;

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
            var performance = new Performance(GetType().Name, $"{Activity}");
            return await performance.MeasureMethodAsync(async () =>
            {
                InternalContract.RequireNotNull(method, nameof(method));
                InternalContract.Require(Activity != null,
                    $"Property {nameof(Activity)} must be set before calling the {nameof(InternalExecuteAsync)} method.");

                await performance.MeasureAsync(
                    async () => await SafeSaveActivityInformationAsync(true, cancellationToken),
                    nameof(SafeSaveActivityInformationAsync));

                // Already have a result?
                if (ActivityPersistence.HasCompleted)
                {
                    return await performance.MeasureAsync(() => SafeGetResultOrThrowAsync(false, ignoreReturnValue,
                        getDefaultValueMethodAsync,
                        cancellationToken));
                }

                if (!string.IsNullOrWhiteSpace(ActivityPersistence.Activity.Instance.AsyncRequestId))
                {
                    throw new HandledRequestPostponedException(ActivityPersistence.Activity.Instance.AsyncRequestId);
                }

                await performance.MeasureAsync(SafeVerifyMaxTimeAsync);

                await performance.MeasureAsync(() =>
                    SafeCallMethodAndUpdateActivityInformationAsync(method, ignoreReturnValue, cancellationToken));

                return await performance.MeasureAsync(() => SafeGetResultOrThrowAsync(false, ignoreReturnValue,
                    getDefaultValueMethodAsync,
                    cancellationToken));
            });
        }

        private async Task SafeUpdateInstanceWithResultAndAlertExceptionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (ActivityPersistence.Activity.Instance.State == ActivityStateEnum.Failed)
                {


                    if (!ActivityPersistence.Activity.Instance.ExceptionAlertHandled.HasValue ||
                        !ActivityPersistence.Activity.Instance.ExceptionAlertHandled.Value)
                    {
                        if (WorkflowVersion is IActivityExceptionAlertHandler alertHandler)
                        {
                            FulcrumAssert.IsNotNull(ActivityPersistence.Activity.Instance.ExceptionCategory,
                                CodeLocation.AsString());
                            var alert = new ActivityExceptionAlert
                            {
                                WorkflowInstanceId = ActivityPersistence.WorkflowPersistence.InstanceId,
                                ActivityInstanceId = ActivityPersistence.Activity.Instance.Id,
                                ExceptionCategory = ActivityPersistence.Activity.Instance.ExceptionCategory!.Value,
                                ExceptionFriendlyMessage = ActivityPersistence.Activity.Instance.ExceptionFriendlyMessage,
                                ExceptionTechnicalMessage = ActivityPersistence.Activity.Instance.ExceptionTechnicalMessage
                            };
                            try
                            {
                                var handled =
                                    await alertHandler.HandleActivityExceptionAlertAsync(alert, cancellationToken);
                                ActivityPersistence.Activity.Instance.ExceptionAlertHandled = handled;
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
                throw new HandledRequestPostponedException(ActivityPersistence.Activity.Instance.AsyncRequestId)
                {
                    TryAgain = true
                };
            }
        }

        private async Task<TMethodReturnType> SafeGetResultOrThrowAsync<TMethodReturnType>(bool publishEvent,
            bool ignoreReturnValue, Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            if (ActivityPersistence.Activity.Instance.State != ActivityStateEnum.Failed)
            {
                return ignoreReturnValue
                    ? default
                    : JsonHelper.SafeDeserializeObject<TMethodReturnType>(ActivityPersistence.Activity.Instance.ResultAsJson);
            }

            if (publishEvent)
            {
                // Publish message about exception
            }

            switch (ActivityPersistence.Activity.Version.FailUrgency)
            {
                case ActivityFailUrgencyEnum.CancelWorkflow:
                    throw new FulcrumCancelledException($"Activity {Activity} failed with the following message: {ActivityPersistence.Activity.Instance.ExceptionTechnicalMessage}");
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
                    ActivityPersistence.Activity.Instance.ResultAsJson = "";
                }
                else
                {
                    var result = await method(Activity, cancellationToken);
                    ActivityPersistence.Activity.Instance.ResultAsJson = result.ToJsonString();
                    ActivityPersistence.Activity.Instance.State = ActivityStateEnum.Success;
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
                ActivityPersistence.Activity.Instance.State = ActivityStateEnum.Waiting;
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
                throw new HandledRequestPostponedException
                {
                    TryAgain = true
                };
            }
            catch (RequestPostponedException e)
            {
                if (e.WaitingForRequestIds == null || e.WaitingForRequestIds.Count != 1) throw;
                ActivityPersistence.Activity.Instance.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
                ActivityPersistence.Activity.Instance.State = ActivityStateEnum.Waiting;
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
                throw new HandledRequestPostponedException(e);
            }
            catch (Exception e)
            {
                // Normal error
                // TODO: Handle error: Send event, throw postpone if halt
                ActivityPersistence.Activity.Instance.State = ActivityStateEnum.Failed;
                ActivityPersistence.Activity.Instance.ExceptionCategory = ActivityExceptionCategoryEnum.Technical;
                ActivityPersistence.Activity.Instance.ExceptionTechnicalMessage =
                    $"A local method throw an exception of type {e.GetType().FullName} and message: {e.Message}";
                ActivityPersistence.Activity.Instance.ExceptionFriendlyMessage =
                    $"A local method failed with the following message: {e.Message}";
            }

            if (ActivityPersistence.Activity.Instance.Id != null)
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
                await ActivityPersistence.PersistAsync(cancellationToken);
                FulcrumAssert.IsNotNull(ActivityPersistence.Activity.Instance.Id, CodeLocation.AsString());
                if (newActivity) ActivityPersistence.WorkflowPersistence.AddActivity(ActivityPersistence.Activity.Instance.Id, Activity);
            }
            catch (Exception)
            {
                // Save failed
                // TODO: Log
                throw new HandledRequestPostponedException(ActivityPersistence.Activity.Instance.AsyncRequestId)
                {
                    TryAgain = true
                };
            }
        }
    }
}