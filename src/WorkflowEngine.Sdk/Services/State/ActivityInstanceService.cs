using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State
{
    public class ActivityInstanceService : IActivityInstanceService
    {
        private readonly IRuntimeTables _runtimeTables;
        private readonly IAsyncRequestMgmtCapability _requestMgmtCapability;

        public ActivityInstanceService(IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability)
        {
            _runtimeTables = runtimeTables;
            _requestMgmtCapability = requestMgmtCapability;
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> CreateWithSpecifiedIdAndReturnAsync(string id, ActivityInstanceCreate item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            var recordCreate = new ActivityInstanceRecordCreate().From(item);
            var result = await _runtimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(idAsGuid, recordCreate, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return new ActivityInstance().From(result);
        }

        /// <inheritdoc />
        public async Task SuccessAsync(string id, ActivityInstanceSuccessResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);

            var record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return;

            record.State = ActivityStateEnum.Success.ToString();
            record.ResultAsJson = result.ResultAsJson;
            record.FinishedAt = DateTimeOffset.UtcNow;

            await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            await _requestMgmtCapability.Execution.ReadyForExecutionAsync(record.WorkflowInstanceId.ToString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task FailedAsync(string id, ActivityInstanceFailedResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);

            var record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return;

            record.State = ActivityStateEnum.Failed.ToString();
            record.ExceptionCategory = result.ExceptionCategory.ToString();
            record.ExceptionTechnicalMessage = result.ExceptionTechnicalMessage;
            record.ExceptionFriendlyMessage = result.ExceptionFriendlyMessage;
            record.FinishedAt = DateTimeOffset.UtcNow;

            await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            await _requestMgmtCapability.Execution.ReadyForExecutionAsync(record.WorkflowInstanceId.ToString(), cancellationToken);
            // TODO: Audit log?
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> UpdateAndReturnAsync(string id, ActivityInstance item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);

            var record = new ActivityInstanceRecord().From(item);
            var result = await _runtimeTables.ActivityInstance.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return new ActivityInstance().From(result);
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);

            var record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new ActivityInstance().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }
    }
}