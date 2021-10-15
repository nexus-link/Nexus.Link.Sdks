using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Model;
using ActivityStateEnum = Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.ActivityStateEnum;
using AsyncExecutionContext = Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model.AsyncExecutionContext;
using PostponeException = Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions.PostponeException;

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
            CancellationToken cancellationToken)
        {
            return InternalExecuteAsync<TMethodReturnType>(method, cancellationToken, false);
        }

        protected async Task InternalExecuteAsync(
            ActivityMethod method,
            CancellationToken cancellationToken)
        {
            await InternalExecuteAsync(async (instance, ct) =>
            {
                await method(instance, ct);
                return Task.FromResult(false);
            }, cancellationToken);
        }

        public TParameter GetArgument<TParameter>(string parameterName)
        {
            return ActivityInformation.MethodHandler.GetArgument<TParameter>(parameterName);
        }

        private async Task<TMethodReturnType> InternalExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            CancellationToken cancellationToken,
            bool ignoreReturnValue)
        {
            try
            {
                // Find existing or create new
                ActivityInformation.Iteration = Iteration;
                await ActivityInformation.PersistAsync(cancellationToken);

                // Already have a result?
                if (ActivityInformation.HasCompleted)
                {
                    return GetResultOrThrow<TMethodReturnType>(ignoreReturnValue);
                }

                // Kolla maxtiden

                if (!string.IsNullOrWhiteSpace(ActivityInformation.AsyncRequestId))
                {
                    var response = await _asyncRequestClient.GetFinalResponseAsync(ActivityInformation.AsyncRequestId, cancellationToken);
                    if (response == null || !response.HasCompleted) throw new RequestPostponedException(ActivityInformation.AsyncRequestId);
                    ActivityInformation.Result.Json = response.Content;
                    if (response.Exception?.Name == null)
                    {
                        ActivityInformation.Result.State = ActivityStateEnum.Success;
                    }
                    else
                    {
                        ActivityInformation.Result.State = ActivityStateEnum.Failed;
                        ActivityInformation.Result.FailUrgency = ActivityFailUrgencyEnum.Stopping;
                        ActivityInformation.Result.ExceptionCategory = ActivityExceptionCategoryEnum.Other;
                        ActivityInformation.Result.ExceptionTechnicalMessage = $"A remote method returned an exception with the name {response.Exception.Name} and message: {response.Exception.Message}";
                        ActivityInformation.Result.ExceptionFriendlyMessage = $"A remote method failed with the following message: {response.Exception.Message}";
                        await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                    }
                    return GetResultOrThrow<TMethodReturnType>(ignoreReturnValue);
                }

                // Call the activity. The method will only return if this is a
                TMethodReturnType result;
                try
                {
                    result = default(TMethodReturnType);
                    if (ignoreReturnValue)
                    {
                        await method(this, cancellationToken);
                        ActivityInformation.Result.Json = "";
                    }
                    else
                    {
                        result = await method(this, cancellationToken);
                        ActivityInformation.Result.Json = result.ToJsonString();
                    }
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
                    throw new ActivityException(ActivityInformation.Result.ExceptionCategory.ToString(),
                        ActivityInformation.Result.ExceptionTechnicalMessage);
                }

                await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                return result;
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
            catch (ActivityException)
            {
                // TODO: Handle error
                throw;
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
                await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                throw new ActivityException(ActivityInformation.Result.ExceptionCategory.ToString(),
                    ActivityInformation.Result.ExceptionTechnicalMessage);
            }
        }

        private TMethodReturnType GetResultOrThrow<TMethodReturnType>(bool ignoreResult)
        {
            if (ActivityInformation.Result.State == ActivityStateEnum.Failed)
            {
                throw new ActivityException(ActivityInformation.Result.ExceptionCategory.ToString(),
                    ActivityInformation.Result.ExceptionTechnicalMessage);
            }

            if (ignoreResult) return default;

            return JsonHelper.SafeDeserializeObject<TMethodReturnType>(ActivityInformation.Result.Json);
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