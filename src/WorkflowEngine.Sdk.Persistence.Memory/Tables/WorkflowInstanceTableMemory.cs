using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    /// <inheritdoc cref="IWorkflowInstanceTable" />
    public class WorkflowInstanceTableMemory : CrudMemory<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>, IWorkflowInstanceTable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowInstanceTableMemory()
        {
            UniqueConstraintMethods += item => new WorkflowInstanceRecordUnique()
            {
                ExecutionId = item.ExecutionId ?? Guid.NewGuid().ToGuidString() // Simulate not caring about null
            };
        }

        /// <inheritdoc />
        public override async Task<WorkflowInstanceRecord> UpdateAndReturnAsync(Guid id, WorkflowInstanceRecord item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var oldItem = await ReadAsync(id, cancellationToken);
            if (oldItem != null)
            {
                InternalContract.RequireAreEqual(oldItem.WorkflowVersionId, item.WorkflowVersionId,
                    $"{nameof(item)}.{nameof(item.WorkflowVersionId)}");
                InternalContract.RequireAreEqual(oldItem.ExecutionId, item.ExecutionId,
                    $"{nameof(item)}.{nameof(item.ExecutionId)}");
            }

            return await base.UpdateAndReturnAsync(id, item, cancellationToken);
        }

        /// <inheritdoc />
        public Task<WorkflowInstanceRecord> ReadByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MemoryItems.Values.FirstOrDefault(record => record.ExecutionId == executionId));
        }
    }
}