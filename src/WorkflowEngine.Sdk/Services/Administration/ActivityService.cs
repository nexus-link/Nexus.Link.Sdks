using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

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

            var activityInstance = await activityInstanceService.ReadAsync(id, cancellationToken);
            if (activityInstance == null) throw new FulcrumNotFoundException(id);

            activityInstance.State = ActivityStateEnum.Waiting;
            activityInstance.ResultAsJson = null;
            activityInstance.ExceptionCategory = null;
            activityInstance.ExceptionTechnicalMessage = null;
            activityInstance.ExceptionFriendlyMessage = null;
            activityInstance.AsyncRequestId = null;
            // TODO: item.ExceptionAlertHandled = null

            await activityInstanceService.UpdateAndReturnAsync(id, activityInstance, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(activityInstance.WorkflowInstanceId, cancellationToken);

            // TODO: Audit log
        }
    }
}