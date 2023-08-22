using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
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
        public async Task SuccessAsync(string id, ActivitySuccessResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var activityInstanceService = _workflowStateCapability.ActivityInstance;
            FulcrumAssert.IsNotNull(activityInstanceService, CodeLocation.AsString());
            var activityInstance = await activityInstanceService.ReadAsync(id, cancellationToken);
            if (activityInstance == null) return;

            activityInstance.State = ActivityStateEnum.Success;
            activityInstance.ResultAsJson = result.ResultAsJson;
            activityInstance.FinishedAt = DateTimeOffset.UtcNow;

            await activityInstanceService.UpdateAndReturnAsync(id, activityInstance, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(activityInstance.WorkflowInstanceId.ToGuidString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task FailedAsync(string id, ActivityFailedResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(result, nameof(result));

            var activityInstanceService = _workflowStateCapability.ActivityInstance;
            FulcrumAssert.IsNotNull(activityInstanceService, CodeLocation.AsString());

            var activityInstance = await activityInstanceService.ReadAsync(id, cancellationToken);
            if (activityInstance == null) return;

            activityInstance.State = ActivityStateEnum.Failed;
            activityInstance.ExceptionCategory = result.ExceptionCategory;
            activityInstance.ExceptionTechnicalMessage = result.ExceptionTechnicalMessage;
            activityInstance.ExceptionFriendlyMessage = result.ExceptionFriendlyMessage;
            activityInstance.FinishedAt = DateTimeOffset.UtcNow;

            await activityInstanceService.UpdateAndReturnAsync(id, activityInstance, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(activityInstance.WorkflowInstanceId.ToGuidString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task RetryAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var activityInstanceService = _workflowStateCapability.ActivityInstance;
            FulcrumAssert.IsNotNull(activityInstanceService, CodeLocation.AsString());
            var workflowInstanceService = _workflowStateCapability.WorkflowInstance;
            FulcrumAssert.IsNotNull(workflowInstanceService, CodeLocation.AsString());

            var activityInstance = await activityInstanceService.ReadAsync(id, cancellationToken);
            if (activityInstance == null) throw new FulcrumNotFoundException(id);
            FulcrumAssert.IsValidated(activityInstance, CodeLocation.AsString());

            var workflowInstance = await workflowInstanceService.ReadAsync(activityInstance.WorkflowInstanceId, cancellationToken);
            FulcrumAssert.IsNotNull(workflowInstance, CodeLocation.AsString());

            if (workflowInstance.State != WorkflowStateEnum.Halted)
            {
                throw new FulcrumBusinessRuleException(
                    $"You can only retry workflows that are in state {nameof(WorkflowStateEnum.Halted)}," +
                    $" but workflow {workflowInstance.Id} is in state {workflowInstance.State}.");
            }

            activityInstance.Reset();
            await activityInstanceService.UpdateAndReturnAsync(id, activityInstance, cancellationToken);

            workflowInstance.State = WorkflowStateEnum.Waiting;
            await workflowInstanceService.UpdateAsync(id, workflowInstance, cancellationToken);

            await _requestMgmtCapability.Request.RetryAsync(activityInstance.WorkflowInstanceId, cancellationToken);

            // TODO: Audit log
        }
    }
}