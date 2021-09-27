using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class WorkflowVersionService : IWorkflowVersionService
    {
        private readonly IConfigurationTables _configurationTables;

        public WorkflowVersionService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string masterId, int dependentId, WorkflowVersionCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var recordCreate = new WorkflowVersionRecordCreate().From(item);
            await _configurationTables.WorkflowVersion.CreateWithSpecifiedIdAsync(idAsGuid, dependentId, recordCreate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowVersion> ReadAsync(string masterId, int dependentId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var record = await _configurationTables.WorkflowVersion.ReadAsync(idAsGuid, dependentId, cancellationToken);
            if (record == null) return null;

            var result = new WorkflowVersion().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string masterId, int dependentId, WorkflowVersion item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var record = new WorkflowVersionRecord().From(item);
            await _configurationTables.WorkflowVersion.UpdateAsync(idAsGuid, dependentId, record, cancellationToken);
        }
    }
}