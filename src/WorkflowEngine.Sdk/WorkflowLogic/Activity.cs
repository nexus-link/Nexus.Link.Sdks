using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Model;
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

        protected Activity(IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            ActivityInformation activityInformation,
            Activity previousActivity,
            Activity parentActivity)
        {
            InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));

            _workflowCapability = workflowCapability;
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
                if (!string.IsNullOrWhiteSpace(ActivityInformation.AsyncRequestId))
                {
                    var response = await _asyncRequestClient.GetFinalResponseAsync(ActivityInformation.AsyncRequestId, cancellationToken);
                    if (response == null) throw new RequestAcceptedException("TODO", ActivityInformation.AsyncRequestId);
                    ActivityInformation.Result.Json = response.Content;
                    ActivityInformation.Result.ExceptionName = response.Exception?.Name;
                    ActivityInformation.Result.ExceptionMessage = response.Exception?.Message;
                    await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                    return GetResultOrThrow<TMethodReturnType>(ignoreReturnValue);
                }

                // Call the activity. The method will only return if this is a
                var result = default(TMethodReturnType);
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
                await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                return result;
            }
            catch (HandledRequestAcceptedException e)
            {
                throw;
            }
            catch (RequestAcceptedException e)
            {
                if (e.OutstandingRequestIds == null || e.OutstandingRequestIds.Count != 1) throw;
                ActivityInformation.AsyncRequestId = e.OutstandingRequestIds.First();
                await ActivityInformation.UpdateInstanceWithRequestIdAsync(cancellationToken);
                throw new HandledRequestAcceptedException(e);
            }
            catch (ActivityException)
            {
                throw;
            }
            catch (Exception e)
            {
                ActivityInformation.Result.ExceptionName = e.GetType().FullName;
                ActivityInformation.Result.ExceptionMessage = e.Message;
                await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                throw new ActivityException(ActivityInformation.Result.ExceptionName,
                    ActivityInformation.Result.ExceptionMessage);
            }
        }

        private TMethodReturnType GetResultOrThrow<TMethodReturnType>(bool ignoreResult)
        {
            if (!string.IsNullOrWhiteSpace(ActivityInformation.Result.ExceptionName))
            {
                throw new ActivityException(ActivityInformation.Result.ExceptionName,
                    ActivityInformation.Result.ExceptionMessage);
            }

            if (ignoreResult) return default;

            return JsonHelper.SafeDeserializeObject<TMethodReturnType>(ActivityInformation.Result.Json);
        }
    }

    internal class HandledRequestAcceptedException : RequestAcceptedException
    {
        public HandledRequestAcceptedException(RequestAcceptedException e)
        :base(e.UrlWhereResponseWillBeMadeAvailable, e.OutstandingRequestIds)
        {
        }
    }
}