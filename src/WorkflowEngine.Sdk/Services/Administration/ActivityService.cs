using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration
{
    /// <inheritdoc />
    public class ActivityService : IActivityService
    {
        private readonly IWorkflowStateCapability _workflowStateCapability;
        private readonly IAsyncRequestMgmtCapability _requestMgmtCapability;

        /// <summary>
        /// Controller
        /// </summary>
        public ActivityService(IWorkflowStateCapability workflowStateCapability, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            _workflowStateCapability = workflowStateCapability;
            _requestMgmtCapability = requestMgmtCapability;
        }

        /// <inheritdoc />
        public async Task SuccessAsync(string activityInstanceId, ActivitySuccessResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));

            var activityInstanceService = _workflowStateCapability.ActivityInstance;
            FulcrumAssert.IsNotNull(activityInstanceService, CodeLocation.AsString());
            var activityInstance = await activityInstanceService.ReadAsync(activityInstanceId, cancellationToken);
            if (activityInstance == null) return;

            activityInstance.State = ActivityStateEnum.Success;
            activityInstance.ResultAsJson = result.ResultAsJson;
            activityInstance.FinishedAt = DateTimeOffset.UtcNow;

            await activityInstanceService.UpdateAndReturnAsync(activityInstanceId, activityInstance, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(activityInstance.WorkflowInstanceId.ToGuidString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task FailedAsync(string activityInstanceId, ActivityFailedResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));
            InternalContract.RequireNotNull(result, nameof(result));

            var activityInstanceService = _workflowStateCapability.ActivityInstance;
            FulcrumAssert.IsNotNull(activityInstanceService, CodeLocation.AsString());

            var activityInstance = await activityInstanceService.ReadAsync(activityInstanceId, cancellationToken);
            if (activityInstance == null) return;

            activityInstance.State = ActivityStateEnum.Failed;
            activityInstance.ExceptionCategory = result.ExceptionCategory;
            activityInstance.ExceptionTechnicalMessage = result.ExceptionTechnicalMessage;
            activityInstance.ExceptionFriendlyMessage = result.ExceptionFriendlyMessage;
            activityInstance.FinishedAt = DateTimeOffset.UtcNow;

            await activityInstanceService.UpdateAndReturnAsync(activityInstanceId, activityInstance, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(activityInstance.WorkflowInstanceId.ToGuidString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task RetryAsync(string activityInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));

            var activityInstanceService = _workflowStateCapability.ActivityInstance;
            FulcrumAssert.IsNotNull(activityInstanceService, CodeLocation.AsString());
            var workflowInstanceService = _workflowStateCapability.WorkflowInstance;
            FulcrumAssert.IsNotNull(workflowInstanceService, CodeLocation.AsString());

            var activityInstance = await activityInstanceService.ReadAsync(activityInstanceId, cancellationToken);
            if (activityInstance == null) throw new FulcrumNotFoundException(activityInstanceId);
            FulcrumAssert.IsValidated(activityInstance, CodeLocation.AsString());

            var workflowInstance = await workflowInstanceService.ReadAsync(activityInstance.WorkflowInstanceId, cancellationToken);
            FulcrumAssert.IsNotNull(workflowInstance, CodeLocation.AsString());

            if (workflowInstance.State != WorkflowStateEnum.Halted && workflowInstance.State != WorkflowStateEnum.Halting)
            {
                throw new FulcrumBusinessRuleException(
                    $"You can only retry workflows that are in state {nameof(WorkflowStateEnum.Halted)} or {nameof(WorkflowStateEnum.Halting)}," +
                    $" but workflow {workflowInstance.Id} is in state {workflowInstance.State}.");
            }

            activityInstance.Reset();
            await activityInstanceService.UpdateAndReturnAsync(activityInstanceId, activityInstance, cancellationToken);

            workflowInstance.State = WorkflowStateEnum.Waiting;
            await workflowInstanceService.UpdateAsync(activityInstanceId, workflowInstance, cancellationToken);

            await _requestMgmtCapability.Request.RetryAsync(activityInstance.WorkflowInstanceId, cancellationToken);

            // TODO: Audit log
        }
    }
}