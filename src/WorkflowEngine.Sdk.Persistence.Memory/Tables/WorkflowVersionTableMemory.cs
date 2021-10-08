using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class WorkflowVersionTableMemory : CrudMemory<WorkflowVersionRecordCreate, WorkflowVersionRecord, Guid>, IWorkflowVersionTable
    {
        public WorkflowVersionTableMemory()
        {
            UniqueConstraintMethods += item => new { item.WorkflowFormId, item.MajorVersion };
        }

        /// <inheritdoc />
        public Task<WorkflowVersionRecord> ReadAsync(Guid workflowFormId, int majorVersion, CancellationToken cancellationToken = default)
        {
            return FindUniqueAsync(
                new SearchDetails<WorkflowVersionRecord>(
                    new WorkflowVersionRecordUnique
                    {
                        WorkflowFormId = workflowFormId,
                        MajorVersion = majorVersion
                    }),
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Guid workflowFormId, int majorVersion, WorkflowVersionRecord record,
            CancellationToken cancellationToken = default)
        {
            var item = await ReadAsync(workflowFormId, majorVersion, cancellationToken);
            if (item == null)
            {
                throw new FulcrumNotFoundException(
                    $"{nameof(WorkflowVersionRecord)} not found for {nameof(WorkflowVersionRecord.WorkflowFormId)} {workflowFormId}" +
                    $" and {nameof(WorkflowVersionRecord.MajorVersion)} {majorVersion}.");
            }

            await UpdateAsync(item.Id, record, cancellationToken);
        }
    }
}