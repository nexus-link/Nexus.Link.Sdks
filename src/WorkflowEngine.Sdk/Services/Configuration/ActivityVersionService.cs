using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.Configuration;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.Configuration
{
    public class ActivityVersionService : IActivityVersionService
    {
        private readonly IConfigurationTables _configurationTables;

        public ActivityVersionService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task<ActivityVersion> CreateWithSpecifiedIdAndReturnAsync(string id, ActivityVersionCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = id.ToGuid();
            var recordCreate = new ActivityVersionRecordCreate().From(item);
            var result = await _configurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(idAsGuid, recordCreate, cancellationToken);
            return new ActivityVersion().From(result);
        }

        /// <inheritdoc />
        public async Task<ActivityVersion> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            
            var idAsGuid = id.ToGuid();
            var record = await _configurationTables.ActivityVersion.ReadAsync(idAsGuid, cancellationToken);
             if (record == null) return null;

            var result = new ActivityVersion().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<ActivityVersion> UpdateAndReturnAsync(string id, ActivityVersion item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = id.ToGuid();

            var record = new ActivityVersionRecord().From(item);
            var result = await _configurationTables.ActivityVersion.UpdateAndReturnAsync(idAsGuid, record, cancellationToken);
            return new ActivityVersion().From(result);
        }
    }
}