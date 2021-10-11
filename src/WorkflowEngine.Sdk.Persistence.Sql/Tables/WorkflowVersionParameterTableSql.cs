using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class WorkflowVersionParameterTableSql: CrudSql<WorkflowVersionParameterRecordCreate, WorkflowVersionParameterRecord>, IWorkflowVersionParameterTable
    {
        public WorkflowVersionParameterTableSql(string connectionString) : base(connectionString, new SqlTableMetadata
        {
            TableName = "WorkflowVersionParameter",
            CreatedAtColumnName = nameof(WorkflowVersionParameterRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowVersionParameterRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowVersionParameterRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowVersionParameterRecord.WorkflowVersionId),
                nameof(WorkflowVersionParameterRecord.Name),
            },
            OrderBy = new List<string> { nameof(WorkflowVersionParameterRecord.RecordCreatedAt) }
        })
        {
        }

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