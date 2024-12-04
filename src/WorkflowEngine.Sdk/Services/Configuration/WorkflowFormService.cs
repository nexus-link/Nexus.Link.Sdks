using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.Configuration;
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
        public async Task CreateWithSpecifiedIdAsync(string id, WorkflowFormCreate item, CancellationToken cancellationToken = default)
        {
            await CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowForm> CreateWithSpecifiedIdAndReturnAsync(string id, WorkflowFormCreate item,
            CancellationToken cancellationToken = default)
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
        public async Task<WorkflowForm> ReadAsync(string id, CancellationToken cancellationToken = default)
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
        public async Task UpdateAsync(string id, WorkflowForm item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = id.ToGuid();
            var record = new WorkflowFormRecord().From(item);
            await _configurationTables.WorkflowForm.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowForm> UpdateAndReturnAsync(string id, WorkflowForm item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = id.ToGuid();
            var record = new WorkflowFormRecord().From(item);
            var resultRecord = await _configurationTables.WorkflowForm.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            var result = new WorkflowForm().From(resultRecord);
            return result;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<WorkflowForm>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            var result = await _configurationTables.WorkflowForm.ReadAllWithPagingAsync(offset, limit, cancellationToken);
            return new PageEnvelope<WorkflowForm>
            {
                PageInfo = result.PageInfo,
                Data = result.Data.Select(x => new WorkflowForm().From(x))
            };
        }
    }
}