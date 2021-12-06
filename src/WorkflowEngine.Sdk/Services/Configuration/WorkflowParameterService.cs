using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Configuration
{
    public class WorkflowParameterService : IWorkflowParameterService
    {
        private readonly IConfigurationTables _configurationTables;

        public WorkflowParameterService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string masterId, string dependentId, WorkflowParameterCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            InternalContract.RequireNotNullOrWhiteSpace(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var recordCreate = new WorkflowVersionParameterRecordCreate().From(item);
            await _configurationTables.WorkflowVersionParameter.CreateAsync(recordCreate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowParameter> ReadAsync(string masterId, string dependentId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            
            var idAsGuid = masterId.ToGuid();
            var record = await _configurationTables.WorkflowVersionParameter.ReadAsync(idAsGuid, dependentId, cancellationToken);
            if (record == null) return null;

            var result = new WorkflowParameter().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<WorkflowParameter>> ReadChildrenWithPagingAsync(string masterId, int offset, int? limit = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            
            var idAsGuid = masterId.ToGuid();
            var records = await _configurationTables.WorkflowVersionParameter.ReadAllWithPagingAsync(idAsGuid, offset, limit, cancellationToken);
            if (records == null) return null;

            var items = records.Data.Select(r => new WorkflowParameter().From(r)).ToArray();
            FulcrumAssert.IsValidated(items, CodeLocation.AsString());

            return new PageEnvelope<WorkflowParameter>
            {
                PageInfo = records.PageInfo,
                Data = items
            };
        }
    }
}