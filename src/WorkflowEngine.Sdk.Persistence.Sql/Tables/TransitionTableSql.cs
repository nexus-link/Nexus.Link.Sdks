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
    public class TransitionTableSql : CrudSql<TransitionRecordCreate, TransitionRecord>, ITransitionTable
    {
        public TransitionTableSql(string connectionString) : base(connectionString, new SqlTableMetadata
        {
            TableName = "Transition",
            CreatedAtColumnName = nameof(TransitionRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(TransitionRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(TransitionRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(TransitionRecord.WorkflowVersionId),
                nameof(TransitionRecord.FromActivityVersionId),
                nameof(TransitionRecord.ToActivityVersionId),
            },
            OrderBy = new List<string> { nameof(TransitionRecord.RecordCreatedAt) }
        })
        {
        }

        public Task<PageEnvelope<TransitionRecord>> ReadChildrenWithPagingAsync(Guid workflowVersionId, int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            return SearchAsync(new SearchDetails<TransitionRecord>(
                new
                {
                    WorkflowVersionId = workflowVersionId
                }), offset, limit, cancellationToken);
        }
    }
}