using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
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
        public Task<WorkflowVersionRecord> FindByFormAndMajorAsync(Guid workflowFormId, int majorVersion, CancellationToken cancellationToken = default)
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
        public async Task UpdateByFormAndMajorAsync(Guid workflowFormId, int majorVersion, WorkflowVersionRecord record,
            CancellationToken cancellationToken = default)
        {
            var item = await FindByFormAndMajorAsync(workflowFormId, majorVersion, cancellationToken);
            if (item == null)
            {
                throw new FulcrumNotFoundException(
                    $"{nameof(WorkflowVersionRecord)} not found for {nameof(WorkflowVersionRecord.WorkflowFormId)} {workflowFormId}" +
                    $" and {nameof(WorkflowVersionRecord.MajorVersion)} {majorVersion}.");
            }

            await UpdateAsync(item.Id, record, cancellationToken);
        }

        public override async Task<WorkflowVersionRecord> UpdateAndReturnAsync(Guid id, WorkflowVersionRecord item,
            CancellationToken cancellationToken = default)
        {
            var oldItem = await ReadAsync(id, cancellationToken);
            if (oldItem != null)
            {
                InternalContract.RequireAreEqual(oldItem.WorkflowFormId, item.WorkflowFormId,
                    $"{nameof(item)}.{nameof(item.WorkflowFormId)}");
                InternalContract.RequireAreEqual(oldItem.MajorVersion, item.MajorVersion,
                    $"{nameof(item)}.{nameof(item.MajorVersion)}");
            }

            return await base.UpdateAndReturnAsync(id, item, cancellationToken);
        }
    }
}