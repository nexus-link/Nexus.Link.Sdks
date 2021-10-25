using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class ActivityVersionTableSql : CrudSql<ActivityVersionRecordCreate, ActivityVersionRecord>, IActivityVersionTable
    {
        public ActivityVersionTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "ActivityVersion",
            CreatedAtColumnName = nameof(ActivityVersionRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(ActivityVersionRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(ActivityVersionRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(ActivityVersionRecord.WorkflowVersionId),
                nameof(ActivityVersionRecord.ActivityFormId),
                nameof(ActivityVersionRecord.Position),
                nameof(ActivityVersionRecord.ParentActivityVersionId),
                nameof(ActivityVersionRecord.FailUrgency),
            },
            OrderBy = new List<string> { nameof(ActivityVersionRecord.RecordCreatedAt) }
        })
        {
        }
    }
}