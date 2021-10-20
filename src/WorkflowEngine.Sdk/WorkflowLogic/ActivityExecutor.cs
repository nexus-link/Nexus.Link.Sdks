﻿using System;
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

            await SafeSaveActivityInformationAsync(cancellationToken);

            // Already have a result?
            if (ActivityInformation.HasCompleted)
            {
                return await SafeGetResultOrThrowAsync(false, ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(ActivityInformation.AsyncRequestId))
            {
                return await SafeGetResponseOrThrowAsync(ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
            }

            await SafeVerifyMaxTimeAsync();

            await SafeCallMethodAndUpdateActivityInformationAsync(method, ignoreReturnValue, cancellationToken);

            return await SafeGetResultOrThrowAsync(false, ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
        }

        private async Task SafeUpdateInstanceWithResultAndAlertExceptionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!ActivityInformation.Result.ExceptionAlertHandled.HasValue || !ActivityInformation.Result.ExceptionAlertHandled.Value)
                {
                    if (WorkflowVersion is IActivityExceptionAlertHandler alertHandler)
                    {
                        FulcrumAssert.IsNotNull(ActivityInformation.Result.ExceptionCategory, CodeLocation.AsString());
                        var alert = new ActivityExceptionAlert
                        {
                            WorkflowInstanceId = ActivityInformation.WorkflowInformation.InstanceId,
                            ActivityInstanceId = ActivityInformation.InstanceId,
                            ExceptionCategory = ActivityInformation.Result.ExceptionCategory!.Value,
                            ExceptionFriendlyMessage = ActivityInformation.Result.ExceptionFriendlyMessage,
                            ExceptionTechnicalMessage = ActivityInformation.Result.ExceptionTechnicalMessage
                        };
                        try
                        {
                            var handled = await alertHandler.HandleActivityExceptionAlertAsync(alert, cancellationToken);
                            ActivityInformation.Result.ExceptionAlertHandled = handled;
                        }
                        catch (Exception)
                        {
                            // We will try again next reentry.
                        }
                    }
                }
                await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
            }
            catch (Exception)
            {
                // TODO: Log
                throw new HandledRequestPostponedException(ActivityInformation.AsyncRequestId)
                {
                    TryAgain = true
                };
            }
        }

        private async Task<TMethodReturnType> SafeGetResultOrThrowAsync<TMethodReturnType>(bool publishEvent,
            bool ignoreReturnValue, Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            if (ActivityInformation.State != ActivityStateEnum.Failed)
            {
                return ignoreReturnValue
                    ? default
                    : JsonHelper.SafeDeserializeObject<TMethodReturnType>(ActivityInformation.Result.Json);
            }

            if (publishEvent)
            {
                // Publish message about exception
            }

            switch (ActivityInformation.FailUrgency)
            {
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
                    ActivityInformation.Result.Json = "";
                }
                else
                {
                    var result = await method(Activity, cancellationToken);
                    ActivityInformation.Result.Json = result.ToJsonString();
                    ActivityInformation.State = ActivityStateEnum.Success;
                }
            }
            catch (FulcrumTryAgainException)
            {
                ActivityInformation.State = ActivityStateEnum.Waiting;
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
                throw new HandledRequestPostponedException
                {
                    TryAgain = true
                };
            }
            catch (ActivityPostponedException)
            {
                throw new HandledRequestPostponedException();
            }
            catch (HandledRequestPostponedException)
            {
                throw;
            }
            catch (RequestPostponedException e)
            {
                if (e.WaitingForRequestIds == null || e.WaitingForRequestIds.Count != 1) throw;
                ActivityInformation.AsyncRequestId = e.WaitingForRequestIds.FirstOrDefault();
                ActivityInformation.State = ActivityStateEnum.Waiting;
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
                throw new HandledRequestPostponedException(e);
            }
            catch (Exception e)
            {
                // Normal error
                // TODO: Handle error: Send event, throw postpone if halt
                ActivityInformation.State = ActivityStateEnum.Failed;
                ActivityInformation.Result.ExceptionCategory = ActivityExceptionCategoryEnum.Other;
                ActivityInformation.Result.ExceptionTechnicalMessage =
                    $"A local method throw an exception of type {e.GetType().FullName} and message: {e.Message}";
                ActivityInformation.Result.ExceptionFriendlyMessage =
                    $"A local method failed with the following message: {e.Message}";
            }

            if (ActivityInformation.InstanceId != null)
            {
                await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
            }
        }

        private static Task SafeVerifyMaxTimeAsync()
        {
            return Task.CompletedTask;
        }

        private async Task<TMethodReturnType> SafeGetResponseOrThrowAsync<TMethodReturnType>(bool ignoreReturnValue, Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync, CancellationToken cancellationToken)
        {
            AsyncHttpResponse response;
            try
            {
                response = await AsyncRequestClient.GetFinalResponseAsync(ActivityInformation.AsyncRequestId,
                    cancellationToken);
            }
            catch (Exception)
            {
                // TODO: Log
                throw new HandledRequestPostponedException(ActivityInformation.AsyncRequestId)
                {
                    TryAgain = true
                };
            }

            if (response == null || !response.HasCompleted)
            {
                // No response yet
                throw new HandledRequestPostponedException(ActivityInformation.AsyncRequestId);
            }

            if (response.Exception?.Name == null)
            {
                ActivityInformation.State = ActivityStateEnum.Success;
                ActivityInformation.Result.Json = response.Content;
            }
            else
            {
                ActivityInformation.State = ActivityStateEnum.Failed;
                ActivityInformation.FailUrgency = ActivityFailUrgencyEnum.Stopping;
                ActivityInformation.Result.ExceptionCategory = ActivityExceptionCategoryEnum.Other;
                ActivityInformation.Result.ExceptionTechnicalMessage =
                    $"A remote method returned an exception with the name {response.Exception.Name} and message: {response.Exception.Message}";
                ActivityInformation.Result.ExceptionFriendlyMessage =
                    $"A remote method failed with the following message: {response.Exception.Message}";
            }

            await SafeUpdateInstanceWithResultAndAlertExceptionsAsync(cancellationToken);
            return await SafeGetResultOrThrowAsync(true, ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
        }

        private async Task SafeSaveActivityInformationAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ActivityInformation.PersistAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Save failed
                // TODO: Log
                throw new HandledRequestPostponedException(ActivityInformation.AsyncRequestId)
                {
                    TryAgain = true
                };
            }
        }
    }
}