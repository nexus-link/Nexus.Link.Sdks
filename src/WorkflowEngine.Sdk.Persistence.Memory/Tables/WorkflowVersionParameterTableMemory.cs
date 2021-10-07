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
    public class WorkflowVersionParameterTableMemory : CrudMemory<WorkflowVersionParameterRecordCreate, WorkflowVersionParameterRecord, Guid>, IWorkflowVersionParameterTable
    {
        /// <inheritdoc />
        public Task<WorkflowVersionParameterRecord> ReadAsync(Guid workflowVersionId, string name, CancellationToken cancellationToken = default)
        {
            return FindUniqueAsync(
                new SearchDetails<WorkflowVersionParameterRecord>(
                    new WorkflowVersionParameterRecordUnique
                    {
                        WorkflowVersionId = workflowVersionId,
                        Name = name
                    }),
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<WorkflowVersionParameterRecord>> ReadAllWithPagingAsync(Guid workflowVersionId, int offset, int? limit, CancellationToken cancellationToken = default)
        {
            return SearchAsync(
                new SearchDetails<WorkflowVersionParameterRecord>(
                    new
                    {
                        WorkflowVersionId = workflowVersionId
                    }),
                offset, limit, cancellationToken);
        }
    }
}