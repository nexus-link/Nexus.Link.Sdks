using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    public class ActivityExecutor
    {
        public IWorkflowImplementationBase WorkflowImplementation { get; }
        public Activity Activity { get; set; }

        public ActivityExecutor(IWorkflowImplementationBase workflowImplementation, Activity activity)
        {
            InternalContract.RequireNotNull(workflowImplementation, nameof(workflowImplementation));
            InternalContract.RequireNotNull(activity, nameof(activity));
            WorkflowImplementation = workflowImplementation;
            Activity = activity;
        }

        public async Task ExecuteAsync(ActivityMethod method, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));

            await SafeExecuteAsync1(
                async (a, ct) =>
                {
                    await method(a, ct);
                    return true;
                },
                true,
                _ => Task.FromResult(false),
                cancellationToken);
        }

        public Task<TMethodReturnType> ExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));

            return SafeExecuteAsync1(method, false, getDefaultValueMethodAsync, cancellationToken);
        }

        private async Task<TMethodReturnType> SafeExecuteAsync1<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            bool ignoreReturnValue,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            try
            {
                return await SafeExecuteAsync2(method, ignoreReturnValue, getDefaultValueMethodAsync,
                    cancellationToken);
            }
            catch (ExceptionTransporter)
            {
                Activity.Instance.State = ActivityStateEnum.Waiting;
                throw;
            }
            catch (Exception e)
            {
                if (Activity.Instance.HasCompleted)
                {
                    Log.LogError(
                        $"The workflow engine encountered an unexpected error. {CodeLocation.AsString()}:\r{e}");
                }
                else
                {
                    Activity.Instance.State = ActivityStateEnum.Failed;
                    Activity.Instance.FinishedAt = DateTimeOffset.UtcNow;
                    ;
                    Activity.Instance.ExceptionCategory = ActivityExceptionCategoryEnum.WorkflowCapabilityError;
                    Activity.Instance.ExceptionTechnicalMessage =
                        "Internal WorkflowEngine error. Try to run this activity again when the engine has been fixed and updated."
                        + " The engine encountered an unexpected exception:\r{e}";
                    Activity.Instance.ExceptionFriendlyMessage =
                        "The workflow engine software encountered an internal error.";
                    await SafeAlertExceptionAsync(cancellationToken);
                }
                throw new ExceptionTransporter(new RequestPostponedException());
            }
        }

        private async Task<TMethodResult> SafeExecuteAsync2<TMethodResult>(
            ActivityMethod<TMethodResult> method,
            bool ignoreReturnValue,
            Func<CancellationToken, Task<TMethodResult>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            // Book keeping
            BookKeeping();
            await Activity.WorkflowCache.SaveAsync(cancellationToken);
            var activityInstance = Activity.Instance;

            // Already have a result?
            if (activityInstance.HasCompleted)
            {
                if (activityInstance.State == ActivityStateEnum.Success) return GetResultValue<TMethodResult>(ignoreReturnValue);
                return await ThrowOrGetDefaultValue(ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
            }

            try
            {
                await CallMethodAndUpdateActivityInformationAsync(method, ignoreReturnValue, cancellationToken);
            }
            catch (ActivityException e)
            {
                Activity.Instance.State = ActivityStateEnum.Failed;
                Activity.Instance.FinishedAt = DateTimeOffset.UtcNow;
                Activity.Instance.ExceptionCategory = e.ExceptionCategory;
                Activity.Instance.ExceptionTechnicalMessage = e.TechnicalMessage;
                Activity.Instance.ExceptionFriendlyMessage = e.FriendlyMessage;
                await SafeAlertExceptionAsync(cancellationToken);
            }

            if (activityInstance.HasCompleted)
            {
                if (activityInstance.State == ActivityStateEnum.Success) return GetResultValue<TMethodResult>(ignoreReturnValue);
                return await ThrowOrGetDefaultValue(ignoreReturnValue, getDefaultValueMethodAsync, cancellationToken);
            }

            activityInstance.State = ActivityStateEnum.Waiting;
            throw new ExceptionTransporter(new RequestPostponedException());
        }

        private async Task CallMethodAndUpdateActivityInformationAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method, bool ignoreReturnValue, CancellationToken cancellationToken)
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
            catch (Exception e)
            {
                switch (e)
                {
                    case ExceptionTransporter:
                    case ActivityException:
                        throw;
                    case WorkflowFailedException:
                    case FulcrumTryAgainException:
                        throw new ExceptionTransporter(e);
                    case ActivityPostponedException:
                        throw new ExceptionTransporter(new RequestPostponedException());
                    case RequestPostponedException rpe:
                        if (rpe.WaitingForRequestIds.Count == 1)
                        {
                            activityInstance.AsyncRequestId = rpe.WaitingForRequestIds.FirstOrDefault();
                        }
                        throw new ExceptionTransporter(rpe);
                    default:
                        throw new ActivityException(ActivityExceptionCategoryEnum.WorkflowImplementationError,
                            $"An activity is only supposed to throw a limited set of exceptions."
                            + $" The activity threw the following unexpected exception of type {e.GetType().Name}:\r{e}",
                            "The workflow implementation encountered an unexpected error. Please contact the workflow developer.");
                }
            }
        }

        private void BookKeeping()
        {
            var activityInstance = Activity.Instance;
            FulcrumAssert.IsNotNull(activityInstance, CodeLocation.AsString());
            FulcrumAssert.IsNotNullOrWhiteSpace(activityInstance.Id);
            Activity.WorkflowCache.AddActivity(activityInstance.Id, Activity);
            Activity.WorkflowCache.LatestActivity = Activity;
            WorkflowStatic.Context.LatestActivity = Activity;
        }

        private TMethodResult GetResultValue<TMethodResult>(bool ignoreReturnValue)
        {
            FulcrumAssert.AreEqual(ActivityStateEnum.Success, Activity.Instance.State, CodeLocation.AsString());
            return ignoreReturnValue
                        ? default
                        : JsonHelper.SafeDeserializeObject<TMethodResult>(Activity.Instance.ResultAsJson);

        }

        private async Task<TMethodReturnType> ThrowOrGetDefaultValue<TMethodReturnType>(
            bool ignoreReturnValue, Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            FulcrumAssert.AreEqual(ActivityStateEnum.Failed, Activity.Instance.State, CodeLocation.AsString());

            await SafeAlertExceptionAsync(cancellationToken);
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
                    if (getDefaultValueMethodAsync == null) return default;
                    try
                    {
                        return await getDefaultValueMethodAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Log.LogError($"The default value method for activity {Activity} threw an exception." +
                                     $" The default value for {typeof(TMethodReturnType).Name} ({default(TMethodReturnType)}) is used instead. The exception:\r{e}");
                        return default;
                    }
                default:
                    throw new FulcrumAssertionFailedException(
                        $"Unexpected {nameof(ActivityFailUrgencyEnum)} value: {Activity.Version.FailUrgency}.",
                        CodeLocation.AsString());
            }
        }

        private async Task SafeAlertExceptionAsync(CancellationToken cancellationToken)
        {
            if (Activity.Instance.State != ActivityStateEnum.Failed) return;
            if (Activity.Instance.ExceptionCategory == null)
            {
                Log.LogError($"Activity.Instance.ExceptionCategory was null. {CodeLocation.AsString()}");
                return;
            }

            if (Activity.Instance.ExceptionAlertHandled.HasValue &&
                Activity.Instance.ExceptionAlertHandled.Value) return;

            if (Activity.ExceptionAlertHandler == null) return;

            var alert = new ActivityExceptionAlert
            {
                Activity = Activity,
                ExceptionCategory = Activity.Instance.ExceptionCategory!.Value,
                ExceptionFriendlyMessage =
                    Activity.Instance.ExceptionFriendlyMessage,
                ExceptionTechnicalMessage =
                    Activity.Instance.ExceptionTechnicalMessage
            };
            try
            {
                var handled =
                    await Activity.ExceptionAlertHandler(alert, cancellationToken);
                Activity.Instance.ExceptionAlertHandled = handled;
            }
            catch (Exception)
            {
                // We will try again next reentry.
            }
        }
    }
}