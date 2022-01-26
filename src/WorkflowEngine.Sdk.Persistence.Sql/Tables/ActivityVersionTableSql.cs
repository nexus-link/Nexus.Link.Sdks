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

        public Task<IEnumerable<ActivityVersionRecord>> SearchByWorkflowVersionIdAsync(Guid workflowVersionId, int limit = Int32.MaxValue,
            CancellationToken cancellationToken = default)
        {
            return StorageHelper.ReadPagesAsync(
                (o, ct) =>
                    SearchAsync(new SearchDetails<ActivityVersionRecord>(new ActivityVersionRecordSearch() { WorkflowVersionId = workflowVersionId }), o, null, ct),
                limit, cancellationToken);
        }
    }
}