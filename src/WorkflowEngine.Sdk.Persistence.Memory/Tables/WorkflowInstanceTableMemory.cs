using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class WorkflowInstanceTableMemory : CrudMemory<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>, IWorkflowInstanceTable
    {

        /// <summary>
        /// Use this in testing for throwing exceptions at create
        /// </summary>
        public Exception OnlyForTest_Create_AlwaysThrowThisException { get; set; }

        /// <summary>
        /// Use this in testing for throwing exceptions at update
        /// </summary>
        public Exception OnlyForTest_Update_AlwaysThrowThisException { get; set; }


        /// <inheritdoc />
        public override Task CreateWithSpecifiedIdAsync(Guid id, WorkflowInstanceRecordCreate item,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (OnlyForTest_Create_AlwaysThrowThisException != null) throw OnlyForTest_Create_AlwaysThrowThisException;
            return base.CreateWithSpecifiedIdAsync(id, item, cancellationToken);
        }
        public override async Task<WorkflowInstanceRecord> UpdateAndReturnAsync(Guid id, WorkflowInstanceRecord item,
            CancellationToken cancellationToken = default)
        {
            if (OnlyForTest_Update_AlwaysThrowThisException != null) throw OnlyForTest_Update_AlwaysThrowThisException;
            var oldItem = await ReadAsync(id, cancellationToken);
            if (oldItem != null)
            {
                InternalContract.RequireAreEqual(oldItem.WorkflowVersionId, item.WorkflowVersionId,
                    $"{nameof(item)}.{nameof(item.WorkflowVersionId)}");
            }

            return await base.UpdateAndReturnAsync(id, item, cancellationToken);
        }

        public Task<PageEnvelope<WorkflowInstanceRecord>> SearchAsync(WorkflowInstanceSearchDetails instanceSearchDetails, int offset = 0, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            // TODO: Can we join to WorkflowVersion from here?
            throw new NotImplementedException();
        }
    }
}