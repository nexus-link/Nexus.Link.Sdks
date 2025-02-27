using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State
{
    /// <inheritdoc />
    public class WorkflowInstanceService : IWorkflowInstanceService
    {
        private readonly IRuntimeTables _runtimeTables;

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowInstanceService(IRuntimeTables runtimeTables, WorkflowOptions defaultWorkflowOptions)
        {
            _runtimeTables = runtimeTables;
            DefaultWorkflowOptions = defaultWorkflowOptions ?? new WorkflowOptions();
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string id, WorkflowInstanceCreate item, CancellationToken cancellationToken = default)
        {
            await CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowInstance> CreateWithSpecifiedIdAndReturnAsync(string id, WorkflowInstanceCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = id.ToGuid();
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

            var idAsGuid = id.ToGuid();
            var record = await _runtimeTables.WorkflowInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new WorkflowInstance().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, WorkflowInstance item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var record = new WorkflowInstanceRecord().From(item);
            await _runtimeTables.WorkflowInstance.UpdateAndReturnAsync(record.Id, record, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowInstance> UpdateAndReturnAsync(string id, WorkflowInstance item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var record = new WorkflowInstanceRecord().From(item);
            var resultRecord = await _runtimeTables.WorkflowInstance.UpdateAndReturnAsync(record.Id, record, cancellationToken);
            var result = new WorkflowInstance().From(resultRecord);
            return result;
        }

        /// <inheritdoc />
        public async Task<Lock<string>> ClaimDistributedLockAsync(string id, TimeSpan? lockTimeSpan = null, string currentLockId = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            var idAsGuid = id.ToGuid();
            var lockIdAsGuid = Guid.Empty;
            if (currentLockId != null) lockIdAsGuid = currentLockId.ToGuid();

            var result = await _runtimeTables.WorkflowInstance.ClaimDistributedLockAsync(idAsGuid, lockTimeSpan, lockIdAsGuid,
                cancellationToken);
            return new Lock<string>().From(result);

        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(string id, string lockId,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            var idAsGuid = id.ToGuid();
            var lockIdAsGuid = lockId.ToGuid();

            return _runtimeTables.WorkflowInstance.ReleaseDistributedLockAsync(idAsGuid, lockIdAsGuid,
                cancellationToken);
        }

        public WorkflowOptions DefaultWorkflowOptions { get; }
    }
}