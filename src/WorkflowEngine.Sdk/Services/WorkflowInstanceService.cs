using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using WorkflowEngine.Persistence.Abstract;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class WorkflowInstanceService : IWorkflowInstanceService
    {
        private readonly IRuntimeTables _runtimeTables;

        public WorkflowInstanceService(IRuntimeTables runtimeTables)
        {
            _runtimeTables = runtimeTables;
        }

        /// <inheritdoc />
        public async Task CreateChildWithSpecifiedIdAsync(string workflowVersionId, string childId, WorkflowInstanceCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var workflowVersionIdAsGuid = MapperHelper.MapToType<Guid, string>(workflowVersionId);
            var childIdAsGuid = MapperHelper.MapToType<Guid, string>(childId);
            var recordCreate = new WorkflowInstanceRecordCreate().From(item);
            await _runtimeTables.WorkflowInstance.CreateChildWithSpecifiedIdAsync(workflowVersionIdAsGuid, childIdAsGuid, recordCreate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowInstance> ReadAsync(string id, CancellationToken cancellationToken = new CancellationToken())
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
    }
}