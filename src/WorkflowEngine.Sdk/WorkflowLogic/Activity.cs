using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    public delegate Task<TMethodReturnType> ActivityMethod<TMethodReturnType>(
        Activity activity,
        CancellationToken cancellationToken);
    public delegate Task ActivityMethod(
        Activity activity,
        CancellationToken cancellationToken);

    public abstract class Activity
    {
        private readonly IWorkflowCapability _workflowCapability;
        private readonly IAsyncRequestClient _asyncRequestClient;

        public ActivityInformation ActivityInformation { get; }
        public Activity ParentActivity { get; protected set; }
        public Activity PreviousActivity { get; }
        // TODO: Should be nullable instead of relying on value 0
        public int? Iteration { get; protected set; }

        public string Identifier => ActivityInformation.InstanceId;

        public string Title
        {
            get
            {
                if (!NestedIterations.Any()) return ActivityInformation.NestedPositionAndTitle;
                var iterations = string.Join(",", NestedIterations);
                return $"{ActivityInformation.NestedPositionAndTitle} [{iterations}]";
            }
        }

        /// <inheritdoc />
        public override string ToString() => Title;

        public List<int> NestedIterations { get; } = new();

        protected Activity(ActivityInformation activityInformation,
            IAsyncRequestClient asyncRequestClient,
            Activity previousActivity,
            Activity parentActivity)
        {
            InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));

            _workflowCapability = activityInformation.WorkflowCapability;
            _asyncRequestClient = asyncRequestClient;
            ActivityInformation = activityInformation;
            PreviousActivity = previousActivity;
            ParentActivity = parentActivity;

            activityInformation.MethodHandler.InstanceTitle = activityInformation.NestedPositionAndTitle;
            if (ParentActivity != null)
            {
                NestedIterations.AddRange(ParentActivity.NestedIterations);
                if (ParentActivity.Iteration is > 0)
                {
                    NestedIterations.Add(ParentActivity.Iteration.Value);
                }
            }
        }

        protected Task<TMethodReturnType> InternalExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            Func<Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            return InternalExecuteAsync<TMethodReturnType>(method, false, getDefaultValueMethodAsync, cancellationToken);
        }

        protected async Task InternalExecuteAsync(
            ActivityMethod method,
            CancellationToken cancellationToken)
        {
            await InternalExecuteAsync(async (instance, ct) =>
            {
                await method(instance, ct);
                return Task.FromResult(false);
            }, true, null, cancellationToken);
        }

        public TParameter GetArgument<TParameter>(string parameterName)
        {
            return ActivityInformation.MethodHandler.GetArgument<TParameter>(parameterName);
        }

        private async Task<TMethodReturnType> InternalExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            bool ignoreReturnValue,
            Func<Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken)
        {
            try
            {
                // Find existing or create new
                ActivityInformation.Iteration = Iteration;
                await ActivityInformation.PersistAsync(cancellationToken);

                // Already have a result?
                if (ActivityInformation.HasCompleted)
                {
                    return await GetResultOrThrowAsync(false);
                }

                // TODO: Kolla maxtiden

                if (!string.IsNullOrWhiteSpace(ActivityInformation.AsyncRequestId))
                {
                    var response = await _asyncRequestClient.GetFinalResponseAsync(ActivityInformation.AsyncRequestId, cancellationToken);
                    if (response == null || !response.HasCompleted) throw new RequestPostponedException(ActivityInformation.AsyncRequestId);
                    if (response.Exception?.Name == null)
                    {
                        ActivityInformation.Result.State = ActivityStateEnum.Success;
                        ActivityInformation.Result.Json = response.Content;
                    }
                    else
                    {
                        ActivityInformation.Result.State = ActivityStateEnum.Failed;
                        ActivityInformation.Result.FailUrgency = ActivityFailUrgencyEnum.Stopping;
                        ActivityInformation.Result.ExceptionCategory = ActivityExceptionCategoryEnum.Other;
                        ActivityInformation.Result.ExceptionTechnicalMessage = $"A remote method returned an exception with the name {response.Exception.Name} and message: {response.Exception.Message}";
                        ActivityInformation.Result.ExceptionFriendlyMessage = $"A remote method failed with the following message: {response.Exception.Message}";
                        // Publish event
                    }
                    await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                    return await GetResultOrThrowAsync(true);
                }

                // Call the activity. The method will only return if this is a method with no external calls.
                try
                {
                    if (ignoreReturnValue)
                    {
                        await method(this, cancellationToken);
                        ActivityInformation.Result.Json = "";
                    }
                    else
                    {
                        var result = await method(this, cancellationToken);
                        ActivityInformation.Result.Json = result.ToJsonString();
                    }
                }
                catch (RequestPostponedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // Normal error
                    // TODO: Handle error: Send event, throw postpone if halt
                    ActivityInformation.Result.State = ActivityStateEnum.Failed;
                    ActivityInformation.Result.FailUrgency = ActivityFailUrgencyEnum.Stopping;
                    ActivityInformation.Result.ExceptionCategory = ActivityExceptionCategoryEnum.Other;
                    ActivityInformation.Result.ExceptionTechnicalMessage = $"A local method throw an exception of type {e.GetType().FullName} and message: {e.Message}";
                    ActivityInformation.Result.ExceptionFriendlyMessage = $"A local method failed with the following message: {e.Message}";
                    await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                }
            }
            catch (HandledRequestPostponedException)
            {
                throw;
            }
            catch (RequestPostponedException e)
            {
                if (e.WaitingForRequestIds == null || e.WaitingForRequestIds.Count != 1) throw;
                ActivityInformation.AsyncRequestId = e.WaitingForRequestIds.First();
                await ActivityInformation.UpdateInstanceWithRequestIdAsync(cancellationToken);
                throw new HandledRequestPostponedException(e);
            }
            catch (Exception e)
            {
                // Unexpected error
                // TODO: Handle error: Send event, throw postpone if halt (set GiveUpAt if max time)
                ActivityInformation.Result.State = ActivityStateEnum.Failed;
                ActivityInformation.Result.FailUrgency = ActivityFailUrgencyEnum.Stopping;
                ActivityInformation.Result.ExceptionCategory = ActivityExceptionCategoryEnum.Other;
                ActivityInformation.Result.ExceptionTechnicalMessage = $"The workflow SDK failed with an exception ({e.GetType().FullName}) with the following message: {e.Message}";
                ActivityInformation.Result.ExceptionFriendlyMessage = $"The workflow SDK failed with the following message: {e.Message}";
                // Publish event
            }
            if (ActivityInformation.InstanceId != null)
            {
                await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
            }
            return await GetResultOrThrowAsync(false);

            #region Local methods

            async Task<TMethodReturnType> GetResultOrThrowAsync(bool publishEvent)
            {
                if (ActivityInformation.Result.State != ActivityStateEnum.Failed)
                {
                    return ignoreReturnValue
                        ? default
                        : JsonHelper.SafeDeserializeObject<TMethodReturnType>(ActivityInformation.Result.Json);
                }

                if (publishEvent)
                {
                    // Publish message about exception
                }

                FulcrumAssert.IsNotNull(ActivityInformation.Result.FailUrgency, CodeLocation.AsString());
                switch (ActivityInformation.Result.FailUrgency!.Value)
                {
                    case ActivityFailUrgencyEnum.Stopping:
                        throw new RequestPostponedException();
                    default:
                        if (getDefaultValueMethodAsync == null) return default;
                        try
                        {
                            return await getDefaultValueMethodAsync();
                        }
                        catch (Exception)
                        {
                            // Errors in the default method overrides stopping.
                            // TODO: How do we convey information about this to the person who has to deal with this stopping activity?
                            // TODO: Log
                            throw new RequestPostponedException();
                        }
                }
            }
            #endregion
        }
    }

    internal class HandledRequestPostponedException : RequestPostponedException
    {
        public HandledRequestPostponedException(RequestPostponedException e)
        : base(e.WaitingForRequestIds)
        {
        }
    }
}