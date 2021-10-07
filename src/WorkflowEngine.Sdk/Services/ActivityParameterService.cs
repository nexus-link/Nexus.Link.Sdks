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
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class ActivityParameterService : IActivityParameterService
    {
        private readonly IConfigurationTables _configurationTables;

        public ActivityParameterService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string masterId, string dependentId, ActivityParameterCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            InternalContract.RequireNotNullOrWhiteSpace(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var recordCreate = new ActivityVersionParameterRecordCreate().From(item);
            await _configurationTables.ActivityVersionParameter.CreateAsync(recordCreate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ActivityParameter> ReadAsync(string masterId, string dependentId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var record = await _configurationTables.ActivityVersionParameter.ReadAsync(idAsGuid, dependentId, cancellationToken);
            if (record == null) return null;

            var result = new ActivityParameter().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<ActivityParameter>> ReadChildrenWithPagingAsync(string masterId, int offset, int? limit = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(masterId);
            var records = await _configurationTables.ActivityVersionParameter.ReadAllWithPagingAsync(idAsGuid, offset, limit, cancellationToken);
            if (records == null) return null;

            var items = records.Data.Select(r => new ActivityParameter().From(r)).ToArray();
            FulcrumAssert.IsValidated(items, CodeLocation.AsString());

            return new PageEnvelope<ActivityParameter>
            {
                PageInfo = records.PageInfo,
                Data = items
            };
        }
    }
}