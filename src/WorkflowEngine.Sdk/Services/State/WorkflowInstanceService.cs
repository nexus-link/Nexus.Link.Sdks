using System;
using System.Threading;
using System.Threading.Tasks;
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
    public class WorkflowInstanceService : IWorkflowInstanceService
    {
        private readonly IRuntimeTables _runtimeTables;

        public WorkflowInstanceService(IRuntimeTables runtimeTables)
        {
            _runtimeTables = runtimeTables;
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string id, WorkflowInstanceCreate item, CancellationToken cancellationToken = default)
        {
            await CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        public async Task<WorkflowInstance> CreateWithSpecifiedIdAndReturnAsync(string id, WorkflowInstanceCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            var recordCreate = new WorkflowInstanceRecordCreate().From(item);
            var record = await _runtimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(idAsGuid, recordCreate, cancellationToken);

            var result = new WorkflowInstance().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<WorkflowInstance> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            var record = await _runtimeTables.WorkflowInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new WorkflowInstance().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        public async Task UpdateAsync(string id, WorkflowInstance item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var record = new WorkflowInstanceRecord().From(item);
            await _runtimeTables.WorkflowInstance.UpdateAsync(record.Id, record, cancellationToken);
        }
    }
}