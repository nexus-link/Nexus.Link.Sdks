using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables;

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
    public async Task<PageEnvelope<WorkflowInstanceRecord>> SearchAsync(WorkflowSearchDetails searchDetails, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(searchDetails, nameof(searchDetails));
        InternalContract.RequireValidated(searchDetails, nameof(searchDetails));

        await using var connection = await Database.NewSqlConnectionAsync(cancellationToken);

        const string selectRows = "SELECT i.*";
        const string selectCount = "SELECT count(*)";
        var query = $" FROM {TableName} i";
        if (!string.IsNullOrWhiteSpace(searchDetails.FormId))
        {
            query += $" JOIN {WorkflowVersionTableSql.TableName} v ON (v.Id = i.{nameof(WorkflowInstanceRecord.WorkflowVersionId)})";
        }

        query += $" WHERE {nameof(WorkflowInstanceRecord.StartedAt)} >= @{nameof(searchDetails.From)}";

        if (searchDetails.To != null)
        {
            query += $" AND {nameof(WorkflowInstanceRecord.StartedAt)} <= @{nameof(searchDetails.To)}";
        }
        if (!string.IsNullOrWhiteSpace(searchDetails.FormId))
        {
            query += $" AND v.{nameof(WorkflowVersionRecord.WorkflowFormId)} = @{nameof(searchDetails.FormId)}";
        }
        if (searchDetails.State.HasValue)
        {
            query += $" AND i.{nameof(WorkflowInstanceRecord.State)} = @{nameof(searchDetails.StateAsString)}";
        }
        // TODO: Title
        if (!string.IsNullOrWhiteSpace(searchDetails.TitlePart))
        {
            query += $" AND lower({nameof(WorkflowInstanceRecord.Title)}) LIKE lower('%' + @{nameof(searchDetails.TitlePart)} + '%')";
        }

        var orderBy = " ORDER BY " + OrderBy(searchDetails.PrimaryOrderBy, searchDetails.AscendingOrder);
        if (searchDetails.SecondaryOrderBy != searchDetails.PrimaryOrderBy) orderBy += ", " + OrderBy(searchDetails.SecondaryOrderBy, searchDetails.AscendingOrder); // TODO: new order boolean
        orderBy += ", Id";

        var countResult = await connection.QuerySingleAsync<int>(selectCount + query, searchDetails);

        var paging = $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
        var result = await connection.QueryAsync<WorkflowInstanceRecord>(selectRows + query + orderBy + paging, searchDetails);

        return new PageEnvelope<WorkflowInstanceRecord>(offset, limit, countResult, result);
    }

    private static string OrderBy(WorkflowSearchOrderByEnum ob, bool asending)
    {
        var result = ob switch
        {
            WorkflowSearchOrderByEnum.Title => nameof(WorkflowInstanceRecord.Title),
            WorkflowSearchOrderByEnum.State => nameof(WorkflowInstanceRecord.State),
            _ => nameof(WorkflowInstanceRecord.RecordCreatedAt)
        };
        result += asending ? " ASC" : " DESC";
        return result;
    }
}