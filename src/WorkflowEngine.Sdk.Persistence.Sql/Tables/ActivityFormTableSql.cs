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
    public class ActivityFormTableSql : CrudSql<ActivityFormRecordCreate, ActivityFormRecord>, IActivityFormTable
    {
        public ActivityFormTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
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
        public Task<IEnumerable<ActivityFormRecord>> SearchByWorkflowFormIdAsync(Guid workflowFormId, int limit = int.MaxValue, CancellationToken cancellationToken = default)
        {
            return StorageHelper.ReadPagesAsync(
                (o, ct) =>
                    SearchAsync(new SearchDetails<ActivityFormRecord>(new ActivityFormRecordSearch { WorkflowFormId = workflowFormId }), o, null, ct),
                limit, cancellationToken);
        }
    }
}