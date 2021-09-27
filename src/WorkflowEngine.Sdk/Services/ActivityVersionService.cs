using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class ActivityVersionService : IActivityVersionService
    {
        private readonly IConfigurationTables _configurationTables;

        public ActivityVersionService(IConfigurationTables configurationTables)
        {
            _configurationTables = configurationTables;
        }

        /// <inheritdoc />
        public async Task<string> CreateChildAsync(string parentId, ActivityVersionCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            InternalContract.RequireAreEqual( parentId, item.WorkflowVersionId, $"{nameof(item)}.{nameof(item.WorkflowVersionId)})");

            
            var parentIdAsGuid = MapperHelper.MapToType<Guid, string>(parentId);
            var recordCreate = new ActivityVersionRecordCreate().From(item);
            var childIdAsGuid = await _configurationTables.ActivityVersion.CreateChildAsync(parentIdAsGuid, recordCreate, cancellationToken);
            var childId = MapperHelper.MapToType<string, Guid>(childIdAsGuid);
            return childId;
        }

        /// <inheritdoc />
        public async Task<ActivityVersion> FindUniqueByWorkflowVersionActivityAsync(string workflowVersionId, string activityId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNullOrWhiteSpace(activityId, nameof(activityId));
            
            var workflowVersionIdAsGuid = MapperHelper.MapToType<Guid, string>(workflowVersionId);
            var activityIdAsGuid = MapperHelper.MapToType<Guid, string>(activityId);
            var record = await _configurationTables.ActivityVersion.FindUniqueAsync(
                new SearchDetails<ActivityVersionRecord>(new {WorkflowVersionId = workflowVersionIdAsGuid, ActivityFormId = activityIdAsGuid}),
                cancellationToken);
             if (record == null) return null;

            var result = new ActivityVersion().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, ActivityVersion item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);

            var record = new ActivityVersionRecord().From(item);
            await _configurationTables.ActivityVersion.UpdateAsync(idAsGuid, record, cancellationToken);
        }
    }
}