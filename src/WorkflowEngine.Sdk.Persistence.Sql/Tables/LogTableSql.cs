using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class LogTableSql : CrudSql<LogRecordCreate, LogRecord>, ILogTable
    {
        public LogTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "Log",
            CreatedAtColumnName = nameof(LogRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(LogRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(LogRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(LogRecord.WorkflowFormId),
                nameof(LogRecord.WorkflowInstanceId),
                nameof(LogRecord.ActivityFormId),
                nameof(LogRecord.SeverityLevel),
                nameof(LogRecord.SeverityLevelNumber),
                nameof(LogRecord.Message),
                nameof(LogRecord.DataAsJson),
                nameof(LogRecord.TimeStamp)
            },
            OrderBy = new List<string> { nameof(LogRecord.RecordCreatedAt) }
        })
        {
        }

        /// <inheritdoc />
        public Task<PageEnvelope<LogRecord>> ReadWorkflowChildrenWithPagingAsync(Guid workflowInstanceId, bool alsoActivityChildren, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(workflowInstanceId, nameof(workflowInstanceId));
            if (alsoActivityChildren)
            {
                return SearchAsync(new SearchDetails<LogRecord>(
                    new
                    {
                        WorkflowInstanceId = workflowInstanceId
                    }), offset, limit, cancellationToken);
            }
            return SearchAsync(new SearchDetails<LogRecord>(
                new
                {
                    WorkflowInstanceId = workflowInstanceId,
                    ActivityFormId = (Guid?)null
                }), offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<LogRecord>> ReadActivityChildrenWithPagingAsync(Guid workflowInstanceId, Guid activityFormId, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(activityFormId, nameof(activityFormId));
            return SearchAsync(new SearchDetails<LogRecord>(
                new
                {
                    WorkflowInstanceId = workflowInstanceId,
                    ActivityFormId = activityFormId
                }), offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteWorkflowChildrenAsync(Guid workflowInstanceId, int? threshold = null,
            CancellationToken cancellationToken = default)
        {
            string @where = $"{nameof(LogRecord.WorkflowInstanceId)}=@workflowInstanceId";
            if (threshold.HasValue)
            {
                @where += $" AND {nameof(LogRecord.SeverityLevelNumber)}<=@threshold";
            }
            return DeleteWhereAsync(
                @where,
                new { WorkflowInstanceId = workflowInstanceId, Threshold = threshold},
                cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteActivityChildrenAsync(Guid workflowInstanceId, Guid activityFormId, int? threshold = null,
            CancellationToken cancellationToken = default)
        {
            string @where = $"{nameof(LogRecord.WorkflowInstanceId)}=@{nameof(LogRecord.WorkflowInstanceId)} AND {nameof(LogRecord.ActivityFormId)}=@{nameof(LogRecord.ActivityFormId)}";
            if (threshold.HasValue)
            {
                @where += $" AND {nameof(LogRecord.SeverityLevelNumber)}<=@threshold";
            }
            return DeleteWhereAsync(
                @where,
                new { WorkflowInstanceId = workflowInstanceId, ActivityFormId = activityFormId, Threshold = threshold },
                cancellationToken);
        }
    }
}