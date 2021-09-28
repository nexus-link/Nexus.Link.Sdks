using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public ActivityInformation ActivityInformation { get; }
        public Activity ParentActivity { get; protected set; }
        public Activity PreviousActivity { get; }
        // TODO: Should be nullable instead of relying on value 0
        public int Iteration { get; protected set; }

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
            ActivityInformation activityInformation,
            Activity previousActivity,
            Activity parentActivity)
        {
            InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));

            _workflowCapability = workflowCapability;
            ActivityInformation = activityInformation;
            PreviousActivity = previousActivity;
            ParentActivity = parentActivity;

            activityInformation.MethodHandler.InstanceTitle = activityInformation.NestedPositionAndTitle;
            if (ParentActivity != null)
            {
                NestedIterations.AddRange(ParentActivity.NestedIterations);
                if (ParentActivity.Iteration > 0)
                {
                    NestedIterations.Add(ParentActivity.Iteration);
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

        private async Task<TMethodReturnType> InternalExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            CancellationToken cancellationToken,
            bool ignoreReturnValue)
        {
            // TODO: Create/update LatestRequest in DB
            // TODO: Create/update Arguments in DB
            //SubRequest subRequest = null;
            //var context = AsyncWorkflowStatic.Context.AsyncExecutionContext;
            try
            {
                // Find existing or create new
                await ActivityInformation.PersistAsync(cancellationToken);

                // Already have a result?
                if (ActivityInformation.HasCompleted)
                {
                    return GetResultOrThrow<TMethodReturnType>(ignoreReturnValue);
                }
                if (!string.IsNullOrWhiteSpace(ActivityInformation.AsyncRequestId))
                {
                    // TODO: Try to read it from AM
                    throw new PostponeException(ActivityInformation.AsyncRequestId);
                }

                // Call the activity
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
            catch (PostponeException e)
            {
                if (e.RequestId == null) throw;
                ActivityInformation.AsyncRequestId = e.RequestId;
                await ActivityInformation.UpdateInstanceWithRequestIdAsync(cancellationToken);
                throw new FulcrumAcceptedException();
            }
            catch (SubRequestException)
            {
                throw;
            }
            catch (FulcrumAcceptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                ActivityInformation.Result.ExceptionType = e.GetType().FullName;
                ActivityInformation.Result.ExceptionMessage = e.Message;
                await ActivityInformation.UpdateInstanceWithResultAsync(cancellationToken);
                throw new SubRequestException(new SubRequest("TODO"));
            }
        }

        private TMethodReturnType GetResultOrThrow<TMethodReturnType>(bool ignoreResult)
        {
            if (!string.IsNullOrWhiteSpace(ActivityInformation.Result.ExceptionType))
            {
                // TODO: SubRequestException should take something like an ActivityResult
                throw new SubRequestException(new SubRequest("TODO"));
            }

            if (ignoreResult) return default;

            return JsonHelper.SafeDeserializeObject<TMethodReturnType>(ActivityInformation.Result.Json);
        }
    }
}