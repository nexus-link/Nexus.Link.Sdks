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
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Administration
{
    public class ActivityService : IActivityService
    {
        private readonly IRuntimeTables _runtimeTables;
        private readonly IWorkflowStateCapability _stateCapability;
        private readonly IAsyncRequestMgmtCapability _requestMgmtCapability;

        /// <summary>
        /// Controller
        /// </summary>
        public ActivityService(IRuntimeTables runtimeTables, IWorkflowStateCapability stateCapability, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            _runtimeTables = runtimeTables;
            _stateCapability = stateCapability;
            _requestMgmtCapability = requestMgmtCapability;
        }

        /// <inheritdoc />
        public async Task SuccessAsync(string id, ActivitySuccessResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var idAsGuid = id.ToGuid();

            var record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return;

            record.State = ActivityStateEnum.Success.ToString();
            record.ResultAsJson = result.ResultAsJson;
            record.FinishedAt = DateTimeOffset.UtcNow;

            await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(record.WorkflowInstanceId.ToGuidString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task FailedAsync(string id, ActivityFailedResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var idAsGuid = id.ToGuid();

            var record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return;

            record.State = ActivityStateEnum.Failed.ToString();
            record.ExceptionCategory = result.ExceptionCategory.ToString();
            record.ExceptionTechnicalMessage = result.ExceptionTechnicalMessage;
            record.ExceptionFriendlyMessage = result.ExceptionFriendlyMessage;
            record.FinishedAt = DateTimeOffset.UtcNow;

            await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(record.WorkflowInstanceId.ToGuidString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task RetryAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var item = await _stateCapability.ActivityInstance.ReadAsync(id, cancellationToken);
            if (item == null) throw new FulcrumNotFoundException(id);

            item.State = ActivityStateEnum.Waiting;
            item.ResultAsJson = null;
            item.ExceptionCategory = null;
            item.ExceptionTechnicalMessage = null;
            item.ExceptionFriendlyMessage = null;
            item.AsyncRequestId = null;
            // TODO: item.ExceptionAlertHandled = null

            await _stateCapability.ActivityInstance.UpdateAndReturnAsync(id, item, cancellationToken);
            await _requestMgmtCapability.Request.RetryAsync(item.WorkflowInstanceId, cancellationToken);

            // TODO: Audit log
        }
    }
}