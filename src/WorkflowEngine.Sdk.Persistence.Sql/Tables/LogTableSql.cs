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
                    ActivityInstanceId = (Guid?)null
                }), offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<LogRecord>> ReadActivityChildrenWithPagingAsync(Guid activityInstanceId, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(activityInstanceId, nameof(activityInstanceId));
            return SearchAsync(new SearchDetails<LogRecord>(
                new
                {
                    ActivityInstanceId = activityInstanceId
                }), offset, limit, cancellationToken);
        }
    }
}