using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Configuration
{
    public class WorkflowVersionService : IWorkflowVersionService
    {
        private readonly IConfigurationTables _configurationTables;

        public WorkflowVersionService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task<WorkflowVersion> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            var record = await _configurationTables.WorkflowVersion.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new WorkflowVersion().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<WorkflowVersion> CreateWithSpecifiedIdAndReturnAsync(string workflowVersionId, WorkflowVersionCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(workflowVersionId);
            var record = new WorkflowVersionRecordCreate().From(item);
            var result = await _configurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(idAsGuid, record, cancellationToken);
            return new WorkflowVersion().From(result);
        }

        /// <inheritdoc />
        public async Task<WorkflowVersion> UpdateAndReturnAsync(string workflowVersionId, WorkflowVersion item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(workflowVersionId);
            var record = new WorkflowVersionRecord().From(item);
            var result = await _configurationTables.WorkflowVersion.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            return new WorkflowVersion().From(result);

        }
    }
}