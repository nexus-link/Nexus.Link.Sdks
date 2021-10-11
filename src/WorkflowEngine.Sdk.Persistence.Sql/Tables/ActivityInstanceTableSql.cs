﻿using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class ActivityInstanceTableSql : CrudSql<ActivityInstanceRecordCreate, ActivityInstanceRecord>, IActivityInstanceTable
    {
        public ActivityInstanceTableSql(string connectionString) : base(connectionString, new SqlTableMetadata
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
                nameof(ActivityInstanceRecord.ExceptionName),
                nameof(ActivityInstanceRecord.ExceptionMessage),
                nameof(ActivityInstanceRecord.AsyncRequestId),
            },
            OrderBy = new List<string> { nameof(ActivityInstanceRecord.RecordCreatedAt) }
        })
        {
        }
    }
}