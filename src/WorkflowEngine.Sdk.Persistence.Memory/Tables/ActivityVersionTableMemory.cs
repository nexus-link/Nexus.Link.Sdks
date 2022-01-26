using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class ActivityVersionTableMemory : CrudMemory<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>, IActivityVersionTable
    {
        public ActivityVersionTableMemory()
        {
            UniqueConstraintMethods += item => new ActivityVersionRecordUnique
            {
                WorkflowVersionId = item.WorkflowVersionId,
                ActivityFormId = item.ActivityFormId
            };
        }

        /// <inheritdoc />
        public Task<IEnumerable<ActivityVersionRecord>> SearchByWorkflowVersionIdAsync(Guid workflowVersionId, int limit = Int32.MaxValue,
            CancellationToken cancellationToken = default)
        {
            return StorageHelper.ReadPagesAsync(
                (o, ct) =>
                    SearchAsync(new SearchDetails<ActivityVersionRecord>(new ActivityVersionRecordSearch() { WorkflowVersionId = workflowVersionId }), o, null, ct),
                limit, cancellationToken);
        }

        public override async Task<ActivityVersionRecord> UpdateAndReturnAsync(Guid id, ActivityVersionRecord item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var oldItem = await ReadAsync(id, cancellationToken);
            if (oldItem != null)
            {
                InternalContract.RequireAreEqual(oldItem.WorkflowVersionId, item.WorkflowVersionId,
                    $"{nameof(item)}.{nameof(item.WorkflowVersionId)}");
                InternalContract.RequireAreEqual(oldItem.ActivityFormId, item.ActivityFormId,
                    $"{nameof(item)}.{nameof(item.ActivityFormId)}");
            }

            return await base.UpdateAndReturnAsync(id, item, cancellationToken);
        }
    }
}