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
    public class ActivityFormService : IActivityFormService
    {
        private readonly IConfigurationTables _configurationTables;

        public ActivityFormService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string id, ActivityFormCreate item, CancellationToken cancellationToken = default)
        {
            await CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        public async Task<ActivityForm> CreateWithSpecifiedIdAndReturnAsync(string id, ActivityFormCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            var recordCreate = new ActivityFormRecordCreate().From(item);
            var record  = await _configurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(idAsGuid, recordCreate, cancellationToken);

            var result = new ActivityForm().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<ActivityForm> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            var record = await _configurationTables.ActivityForm.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new ActivityForm().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, ActivityForm item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);

            var record = new ActivityFormRecord().From(item);
            await _configurationTables.ActivityForm.UpdateAsync(idAsGuid, record, cancellationToken);
        }
    }
}