using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class WorkflowInstanceTableSql : CrudSql<WorkflowInstanceRecordCreate, WorkflowInstanceRecord>, IWorkflowInstanceTable
    {
        public WorkflowInstanceTableSql(string connectionString) : base(connectionString, new SqlTableMetadata
        {
            TableName = "WorkflowInstance",
            CreatedAtColumnName = nameof(WorkflowInstanceRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowInstanceRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowInstanceRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowInstanceRecord.WorkflowVersionId),
                nameof(WorkflowInstanceRecord.Title),
                nameof(WorkflowInstanceRecord.InitialVersion),
                nameof(WorkflowInstanceRecord.StartedAt),
                nameof(WorkflowInstanceRecord.FinishedAt),
            },
            OrderBy = new List<string> { nameof(WorkflowInstanceRecord.RecordCreatedAt) }
        })
        {
        }
    }
}