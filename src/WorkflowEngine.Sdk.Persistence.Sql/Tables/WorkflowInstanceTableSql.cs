﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class WorkflowInstanceTableSql : CrudSql<WorkflowInstanceRecordCreate, WorkflowInstanceRecord>, IWorkflowInstanceTable
    {
        public WorkflowInstanceTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "WorkflowInstance",
            CreatedAtColumnName = nameof(WorkflowInstanceRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowInstanceRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowInstanceRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowInstanceRecord.WorkflowVersionId),
                nameof(WorkflowInstanceRecord.ExecutionId),
                nameof(WorkflowInstanceRecord.Title),
                nameof(WorkflowInstanceRecord.InitialVersion),
                nameof(WorkflowInstanceRecord.StartedAt),
                nameof(WorkflowInstanceRecord.FinishedAt),
                nameof(WorkflowInstanceRecord.CancelledAt),
                nameof(WorkflowInstanceRecord.IsComplete),
                nameof(WorkflowInstanceRecord.ResultAsJson),
                nameof(WorkflowInstanceRecord.ExceptionFriendlyMessage),
                nameof(WorkflowInstanceRecord.ExceptionTechnicalMessage),
                nameof(WorkflowInstanceRecord.State),
            },
            OrderBy = new List<string> { nameof(WorkflowInstanceRecord.RecordCreatedAt) },
            UpdateCanUseOutput = false
        })
        {
        }

        /// <inheritdoc />
        public async Task<WorkflowInstanceRecord> ReadByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default)
        {
            var search = new SearchDetails<WorkflowInstanceRecord>(new WorkflowInstanceRecordUnique
            {
                ExecutionId = executionId
            });
            var instance = await FindUniqueAsync(search, cancellationToken);
            return instance;
        }
    }
}