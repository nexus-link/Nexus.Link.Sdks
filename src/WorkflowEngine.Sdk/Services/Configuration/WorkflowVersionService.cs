using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.Configuration;
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
            
            var idAsGuid = id.ToGuid();
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

            var idAsGuid = workflowVersionId.ToGuid();
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

            var idAsGuid = workflowVersionId.ToGuid();
            var oldRecord = await _configurationTables.WorkflowVersion.ReadAsync(idAsGuid, cancellationToken);
            var record = new WorkflowVersionRecord().From(item);
            InternalContract.RequireAreEqual(oldRecord.MajorVersion, record.MajorVersion, nameof(item), 
                $"Expected parameter {nameof(item)} to have the same {nameof(record.MajorVersion)} as the current value ({oldRecord.MajorVersion}), but it had the value {record.MajorVersion}");
            var result = await _configurationTables.WorkflowVersion.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            return new WorkflowVersion().From(result);

        }
    }
}