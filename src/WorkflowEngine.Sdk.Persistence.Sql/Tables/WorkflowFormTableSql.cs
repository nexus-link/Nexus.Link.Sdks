using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class WorkflowFormTableSql : CrudSql<WorkflowFormRecordCreate, WorkflowFormRecord>, IWorkflowFormTable
    {
        public WorkflowFormTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "WorkflowForm",
            CreatedAtColumnName = nameof(WorkflowFormRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowFormRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowFormRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowFormRecord.CapabilityName),
                nameof(WorkflowFormRecord.Title),
            },
            OrderBy = new List<string> { nameof(WorkflowFormRecord.RecordCreatedAt) }
        })
        {
        }
    }
}