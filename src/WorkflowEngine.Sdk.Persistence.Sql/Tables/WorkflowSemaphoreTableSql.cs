using System.Collections.Generic;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    /// <inheritdoc cref="IWorkflowSemaphoreTable" />
    public class WorkflowSemaphoreTableSql : CrudSql<WorkflowSemaphoreRecordCreate, WorkflowSemaphoreRecord>, IWorkflowSemaphoreTable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowSemaphoreTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "WorkflowSemaphore",
            CreatedAtColumnName = nameof(WorkflowSemaphoreRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowSemaphoreRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowSemaphoreRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowSemaphoreRecord.WorkflowFormId),
                nameof(WorkflowSemaphoreRecord.ResourceIdentifier),
                nameof(WorkflowSemaphoreRecord.Limit)
            },
            OrderBy = new List<string> { nameof(WorkflowSemaphoreRecord.RecordCreatedAt) },
            UpdateCanUseOutput = false
        })
        {
        }
    }
}