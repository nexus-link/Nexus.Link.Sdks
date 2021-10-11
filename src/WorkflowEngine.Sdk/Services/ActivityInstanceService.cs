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
    public class ActivityInstanceService : IActivityInstanceService
    {
        private readonly IRuntimeTables _runtimeTables;

        public ActivityInstanceService(IRuntimeTables runtimeTables)
        {
            _runtimeTables = runtimeTables;
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(ActivityInstanceCreate item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            
            var recordCreate = new ActivityInstanceRecordCreate().From(item);
            recordCreate.StartedAt = DateTimeOffset.UtcNow;
            var childIdAsGuid = await _runtimeTables.ActivityInstance.CreateAsync(recordCreate, cancellationToken);
            var childId = MapperHelper.MapToType<string, Guid>(childIdAsGuid);
            return childId;
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> FindUniqueAsync(ActivityInstanceUnique findUnique, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(findUnique, nameof(findUnique));
            
            var record = await _runtimeTables.ActivityInstance.FindUniqueAsync(
                new SearchDetails<ActivityInstanceRecord>(
                    new {
                        WorkflowInstanceId = MapperHelper.MapToType<Guid, string>(findUnique.WorkflowInstanceId),
                        ActivityVersionId = MapperHelper.MapToType<Guid, string>(findUnique.ActivityVersionId),
                        ParentActivityInstanceId = MapperHelper.MapToType<Guid?, string>(findUnique.ParentActivityInstanceId),
                        findUnique.ParentIteration
                    }),
                cancellationToken);
             if (record == null) return null;

            var result = new ActivityInstance().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, ActivityInstance item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            
            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);

            var record = new ActivityInstanceRecord().From(item);
            await _runtimeTables.ActivityInstance.UpdateAsync(idAsGuid, record, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ActivityInstance> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            var idAsGuid = MapperHelper.MapToType<Guid, string>(id);
            
            var record = await _runtimeTables.ActivityInstance.ReadAsync(idAsGuid, cancellationToken);
            if (record == null) return null;

            var result = new ActivityInstance().From(record);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }
    }
}