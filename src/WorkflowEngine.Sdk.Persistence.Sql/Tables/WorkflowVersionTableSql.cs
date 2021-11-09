using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class WorkflowVersionTableSql : CrudSql<WorkflowVersionRecordCreate, WorkflowVersionRecord>, IWorkflowVersionTable
    {
        public WorkflowVersionTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "WorkflowVersion",
            CreatedAtColumnName = nameof(WorkflowVersionRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowVersionRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowVersionRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowVersionRecord.WorkflowFormId),
                nameof(WorkflowVersionRecord.MajorVersion),
                nameof(WorkflowVersionRecord.MinorVersion),
                nameof(WorkflowVersionRecord.DynamicCreate),
            },
            OrderBy = new List<string> { nameof(WorkflowVersionRecord.RecordCreatedAt) }
        })
        {
        }

        public Task<WorkflowVersionRecord> ReadByFormAndMajorAsync(Guid workflowFormId, int majorVersion, CancellationToken cancellationToken = default)
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

        public async Task UpdateByFormAndMajorAsync(Guid workflowFormId, int majorVersion, WorkflowVersionRecord record, CancellationToken cancellationToken = default)
        {
            var item = await ReadByFormAndMajorAsync(workflowFormId, majorVersion, cancellationToken);
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