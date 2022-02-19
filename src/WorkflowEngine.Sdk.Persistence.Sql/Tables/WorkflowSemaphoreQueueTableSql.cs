using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    /// <inheritdoc cref="IWorkflowSemaphoreQueueTable" />
    public class WorkflowSemaphoreQueueTableSql : CrudSql<WorkflowSemaphoreQueueRecordCreate, WorkflowSemaphoreQueueRecord>, IWorkflowSemaphoreQueueTable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowSemaphoreQueueTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "WorkflowSemaphoreQueue",
            CreatedAtColumnName = nameof(WorkflowSemaphoreQueueRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowSemaphoreQueueRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowSemaphoreQueueRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowSemaphoreQueueRecord.WorkflowSemaphoreId),
                nameof(WorkflowSemaphoreQueueRecord.WorkflowInstanceId)
            },
            OrderBy = new List<string> { nameof(WorkflowSemaphoreQueueRecord.RecordCreatedAt) },
            UpdateCanUseOutput = false
        })
        {
        }
    }
}