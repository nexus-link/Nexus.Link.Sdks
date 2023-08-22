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
    public class ActivityFormTableMemory : CrudMemory<ActivityFormRecordCreate, ActivityFormRecord, Guid>, IActivityFormTable
    {
        public Task<IEnumerable<ActivityFormRecord>> SearchByWorkflowFormIdAsync(Guid workflowFormId, int limit = int.MaxValue, CancellationToken cancellationToken = default)
        {
            return StorageHelper.ReadPagesAsync(
                (o, ct) =>
                SearchAsync(new SearchDetails<ActivityFormRecord>(new ActivityFormRecordSearch { WorkflowFormId = workflowFormId }), o, null, ct),
                limit, cancellationToken);
        }

        public override async Task<ActivityFormRecord> UpdateAndReturnAsync(Guid id, ActivityFormRecord item,
            CancellationToken cancellationToken = default)
        {
            var oldItem = await ReadAsync(id, cancellationToken);
            if (oldItem != null)
            {
                InternalContract.RequireAreEqual(oldItem.WorkflowFormId, item.WorkflowFormId,
                    $"{nameof(item)}.{nameof(item.WorkflowFormId)}");
                InternalContract.RequireAreEqual(oldItem.Type, item.Type,
                    $"{nameof(item)}.{nameof(item.Type)}");
            }

            return await base.UpdateAndReturnAsync(id, item, cancellationToken);
        }
    }
}