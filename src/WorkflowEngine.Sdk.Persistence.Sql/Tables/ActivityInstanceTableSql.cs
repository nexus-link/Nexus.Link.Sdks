using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class ActivityInstanceTableSql : CrudSql<ActivityInstanceRecordCreate, ActivityInstanceRecord>, IActivityInstanceTable
    {
        public ActivityInstanceTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "ActivityInstance",
            CreatedAtColumnName = nameof(ActivityInstanceRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(ActivityInstanceRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(ActivityInstanceRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(ActivityInstanceRecord.WorkflowInstanceId),
                nameof(ActivityInstanceRecord.ActivityVersionId),
                nameof(ActivityInstanceRecord.ParentActivityInstanceId),
                nameof(ActivityInstanceRecord.ParentIteration),
                nameof(ActivityInstanceRecord.StartedAt),
                nameof(ActivityInstanceRecord.FinishedAt),
                nameof(ActivityInstanceRecord.HasCompleted),
                nameof(ActivityInstanceRecord.ResultAsJson),
                nameof(ActivityInstanceRecord.State),
                nameof(ActivityInstanceRecord.FailUrgency),
                nameof(ActivityInstanceRecord.ExceptionCategory),
                nameof(ActivityInstanceRecord.ExceptionFriendlyMessage),
                nameof(ActivityInstanceRecord.ExceptionTechnicalMessage),
                nameof(ActivityInstanceRecord.AsyncRequestId),
            },
            OrderBy = new List<string> { nameof(ActivityInstanceRecord.RecordCreatedAt) }
        })
        {
        }
    }
}