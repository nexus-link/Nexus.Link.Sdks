using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Configuration
{
    public class WorkflowFormService : IWorkflowFormService
    {
        private readonly IConfigurationTables _configurationTables;

        public WorkflowFormService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }
        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string id, WorkflowFormCreate item, CancellationToken cancellationToken = new CancellationToken())
        {
            await CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowForm> CreateWithSpecifiedIdAndReturnAsync(string id, WorkflowFormCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            
            var idAsGuid = id.ToGuid();
            var recordCreate = new WorkflowFormRecordCreate().From(item);
            var record = await _configurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(idAsGuid, recordCreate, cancellationToken);

            var result = new WorkflowForm().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<WorkflowForm> ReadAsync(string id, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            
            var idAsGuid = id.ToGuid();
            var record = await _configurationTables.WorkflowForm.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new WorkflowForm().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, WorkflowForm item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = id.ToGuid();
            var record = new WorkflowFormRecord().From(item);
            await _configurationTables.WorkflowForm.UpdateAsync(idAsGuid, record, cancellationToken);
        }
    }
}