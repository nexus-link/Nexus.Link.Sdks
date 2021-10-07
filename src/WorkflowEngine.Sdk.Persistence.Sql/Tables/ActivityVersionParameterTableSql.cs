using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class ActivityVersionParameterTableSql : CrudSql<ActivityVersionParameterRecordCreate, ActivityVersionParameterRecord>, IActivityVersionParameterTable
    {
        public ActivityVersionParameterTableSql(string connectionString) : base(connectionString, new SqlTableMetadata
        {
            TableName = "ActivityVersionParameter",
            CreatedAtColumnName = nameof(ActivityVersionParameterRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(ActivityVersionParameterRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(ActivityVersionParameterRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(ActivityVersionParameterRecord.ActivityVersionId),
                nameof(ActivityVersionParameterRecord.Name),
            },
            OrderBy = new List<string> { nameof(ActivityVersionParameterRecord.RecordCreatedAt) }
        })
        {
        }

        /// <inheritdoc />
        public Task<ActivityVersionParameterRecord> ReadAsync(Guid activityVersionId, string name, CancellationToken cancellationToken = default)
        {
            return FindUniqueAsync(
                new SearchDetails<ActivityVersionParameterRecord>(
                    new ActivityVersionParameterRecordUnique
                    {
                        ActivityVersionId = activityVersionId,
                        Name = name
                    }),
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<ActivityVersionParameterRecord>> ReadAllWithPagingAsync(Guid activityVersionId, int offset, int? limit, CancellationToken cancellationToken = default)
        {
            return SearchAsync(
                new SearchDetails<ActivityVersionParameterRecord>(
                    new
                    {
                        ActivityVersionId = activityVersionId
                    }),
                offset, limit, cancellationToken);
        }

    }
}