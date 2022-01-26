using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class WorkflowInstanceTableMemory : CrudMemory<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>, IWorkflowInstanceTable
    {
        public override async Task<WorkflowInstanceRecord> UpdateAndReturnAsync(Guid id, WorkflowInstanceRecord item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var oldItem = await ReadAsync(id, cancellationToken);
            if (oldItem != null)
            {
                InternalContract.RequireAreEqual(oldItem.WorkflowVersionId, item.WorkflowVersionId,
                    $"{nameof(item)}.{nameof(item.WorkflowVersionId)}");
            }

            return await base.UpdateAndReturnAsync(id, item, cancellationToken);
        }
    }
}