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
    public class TransitionTableMemory : CrudMemory<TransitionRecordCreate, TransitionRecord, Guid>, ITransitionTable
    {
        /// <inheritdoc />
        public Task<PageEnvelope<TransitionRecord>> ReadChildrenWithPagingAsync(Guid workflowVersionId, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            return SearchAsync(new SearchDetails<TransitionRecord>(
                new
                {
                    WorkflowVersionId = workflowVersionId
                }), offset, limit, cancellationToken);
        }
    }
}