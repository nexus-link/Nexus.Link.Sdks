using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class ActivityFormTableSql : CrudSql<ActivityFormRecordCreate, ActivityFormRecord>, IActivityFormTable
    {
        public ActivityFormTableSql(string connectionString) : base(connectionString, new SqlTableMetadata
        {
            TableName = "ActivityForm",
            CreatedAtColumnName = nameof(ActivityFormRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(ActivityFormRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(ActivityFormRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(ActivityFormRecord.WorkflowFormId),
                nameof(ActivityFormRecord.Type),
                nameof(ActivityFormRecord.Title),
            },
            OrderBy = new List<string> { nameof(ActivityFormRecord.RecordCreatedAt) }
        })
        {
        }
    }
}