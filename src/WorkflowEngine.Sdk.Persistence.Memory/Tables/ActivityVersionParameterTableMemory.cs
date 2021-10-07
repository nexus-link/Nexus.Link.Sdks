using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class ActivityVersionParameterTableMemory : CrudMemory<ActivityVersionParameterRecordCreate, ActivityVersionParameterRecord, Guid>, IActivityVersionParameterTable
    {
        /// <inheritdoc />
        public Task<ActivityVersionParameterRecord> ReadAsync(Guid activityVersionId, string name, CancellationToken cancellationToken = default)
        {
            return FindUniqueAsync(
                new SearchDetails<ActivityVersionParameterRecord>(
                    new ActivityVersionParameterRecordUnique
                    {
                        ActivityVersionId = activityVersionId,
                        Name = name
                    }),
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<ActivityVersionParameterRecord>> ReadAllWithPagingAsync(Guid activityVersionId, int offset, int? limit, CancellationToken cancellationToken = default)
        {
            return SearchAsync(
                new SearchDetails<ActivityVersionParameterRecord>(
                    new
                    {
                        ActivityVersionId = activityVersionId
                    }),
                offset, limit, cancellationToken);
        }
    }
}