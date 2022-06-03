using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.Model;
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
                nameof(ActivityInstanceRecord.AbsolutePosition),
                nameof(ActivityInstanceRecord.StartedAt),
                nameof(ActivityInstanceRecord.FinishedAt),
                nameof(ActivityInstanceRecord.ResultAsJson),
                nameof(ActivityInstanceRecord.ContextAsJson),
                nameof(ActivityInstanceRecord.State),
                nameof(ActivityInstanceRecord.ExceptionAlertHandled),
                nameof(ActivityInstanceRecord.ExceptionCategory),
                nameof(ActivityInstanceRecord.ExceptionFriendlyMessage),
                nameof(ActivityInstanceRecord.ExceptionTechnicalMessage),
                nameof(ActivityInstanceRecord.AsyncRequestId),
            },
            OrderBy = new List<string> { nameof(ActivityInstanceRecord.RecordCreatedAt) },
            UpdateCanUseOutput = false
        })
        {
        }

        /// <inheritdoc />
        public Task<IEnumerable<ActivityInstanceRecord>> SearchByWorkflowInstanceIdAsync(Guid workflowInstanceId, int limit = Int32.MaxValue,
            CancellationToken cancellationToken = default)
        {
            return StorageHelper.ReadPagesAsync(
                (o, ct) =>
                    SearchAsync(new SearchDetails<ActivityInstanceRecord>(new ActivityInstanceRecordSearch() { WorkflowInstanceId = workflowInstanceId }), o, null, ct),
                limit, cancellationToken);
        }
    }
}