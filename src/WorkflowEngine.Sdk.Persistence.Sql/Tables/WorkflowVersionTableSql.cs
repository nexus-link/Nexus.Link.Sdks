using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class WorkflowVersionTableSql : CrudSql<WorkflowVersionRecordCreate, WorkflowVersionRecord>, IWorkflowVersionTable
    {
        public WorkflowVersionTableSql(string connectionString) : base(connectionString, new SqlTableMetadata
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

        public async Task<WorkflowVersionRecord> ReadAsync(Guid workflowFormId, int majorVersion, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Guid workflowFormId, int majorVersion, WorkflowVersionRecord record, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}