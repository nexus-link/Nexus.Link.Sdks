using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using WorkflowEngine.Persistence.Abstract;
using WorkflowEngine.Persistence.Abstract.Entities;
using WorkflowEngine.Sdk.Support;

namespace WorkflowEngine.Sdk.Services
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
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var recordCreate = new MethodParameterRecordCreate().From(item);
            await _configurationTables.MethodParameter.CreateWithSpecifiedIdAsync(idAsGuid, dependentId, recordCreate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowParameter> ReadAsync(string masterId, string dependentId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var record = await _configurationTables.MethodParameter.ReadAsync(idAsGuid, dependentId, cancellationToken);
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
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var records = await _configurationTables.MethodParameter.ReadChildrenWithPagingAsync(idAsGuid, offset, limit, cancellationToken);
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