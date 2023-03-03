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
    public class ActivityInstanceTableMemory : CrudMemory<ActivityInstanceRecordCreate, ActivityInstanceRecord, Guid>, IActivityInstanceTable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ActivityInstanceTableMemory()
        {
            UniqueConstraintMethods += item => new ActivityInstanceRecordUnique
            {
                WorkflowInstanceId = item.WorkflowInstanceId,
                ParentActivityInstanceId = item.ParentActivityInstanceId,
                ActivityVersionId = item.ActivityVersionId,
                ParentIteration = item.ParentIteration
            };
        }

        /// <inheritdoc />
        public Task<IEnumerable<ActivityInstanceRecord>> SearchByWorkflowInstanceIdAsync(Guid workflowInstanceId, int limit = Int32.MaxValue,
            CancellationToken cancellationToken = default)
        {
            return StorageHelper.ReadPagesAsync(
                (o, ct) =>
                    SearchAsync(new SearchDetails<ActivityInstanceRecord>(new ActivityInstanceRecordSearch() { WorkflowInstanceId = workflowInstanceId }), o, null, ct),
                limit, cancellationToken);
        }

        public override async Task<ActivityInstanceRecord> UpdateAndReturnAsync(Guid id, ActivityInstanceRecord item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var oldItem = await ReadAsync(id, cancellationToken);
            if (oldItem != null)
            {
                InternalContract.RequireAreEqual(oldItem.WorkflowInstanceId, item.WorkflowInstanceId,
                    $"{nameof(item)}.{nameof(item.WorkflowInstanceId)}");
                InternalContract.RequireAreEqual(oldItem.ActivityVersionId, item.ActivityVersionId,
                    $"{nameof(item)}.{nameof(item.ActivityVersionId)}");
                InternalContract.RequireAreEqual(oldItem.ParentActivityInstanceId, item.ParentActivityInstanceId,
                    $"{nameof(item)}.{nameof(item.ParentActivityInstanceId)}");
                InternalContract.RequireAreEqual(oldItem.ParentIteration, item.ParentIteration,
                    $"{nameof(item)}.{nameof(item.ParentIteration)}");
                InternalContract.RequireAreEqual(oldItem.StartedAt, item.StartedAt,
                    $"{nameof(item)}.{nameof(item.StartedAt)}");
            }

            return await base.UpdateAndReturnAsync(id, item, cancellationToken);
        }
    }
}