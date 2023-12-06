using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables;

/// <inheritdoc cref="IWorkflowInstanceTable" />
public class WorkflowInstanceTableSql : CrudSql<WorkflowInstanceRecordCreate, WorkflowInstanceRecord>, IWorkflowInstanceTable
{
    /// <summary>
    /// Constructor
    /// </summary>
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
    public async Task<PageEnvelope<WorkflowInstanceRecord>> SearchAsync(WorkflowInstanceSearchDetails searchDetails, int offset = 0, int? limit = null, CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNull(searchDetails, nameof(searchDetails));
        InternalContract.RequireValidated(searchDetails, nameof(searchDetails));

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
        if (searchDetails.States != null && searchDetails.States.Any())
        {
            query += $" AND i.{nameof(WorkflowInstanceRecord.State)} IN ({string.Join(",", searchDetails.States.Select(x => $"'{x}'"))})";
        }
        if (!string.IsNullOrWhiteSpace(searchDetails.TitlePart))
        {
            query += $" AND lower({nameof(WorkflowInstanceRecord.Title)}) LIKE lower('%' + @{nameof(searchDetails.TitlePart)} + '%')";
        }

        var orderBy = " " + OrderBy(searchDetails.Order.PrimaryOrderBy, searchDetails.Order.PrimaryAscendingOrder);
        if (searchDetails.Order.SecondaryOrderBy.HasValue && searchDetails.Order.SecondaryOrderBy != searchDetails.Order.PrimaryOrderBy)
        {
            orderBy += ", " + OrderBy(searchDetails.Order.SecondaryOrderBy.Value, searchDetails.Order.SecondaryAscendingOrder);
        }
        orderBy += $", {nameof(WorkflowInstanceRecord.Id)}";

        return await SearchAdvancedAsync(selectCount, selectRows, query, orderBy, searchDetails, offset, limit, cancellationToken);
    }

    private static string OrderBy(WorkflowSearchOrderByEnum ob, bool asending)
    {
        var result = ob switch
        {
            WorkflowSearchOrderByEnum.Title => nameof(WorkflowInstanceRecord.Title),
            WorkflowSearchOrderByEnum.State => nameof(WorkflowInstanceRecord.State),
            WorkflowSearchOrderByEnum.FinishedAt => nameof(WorkflowInstanceRecord.FinishedAt),
            _ => nameof(WorkflowInstanceRecord.StartedAt)
        };
        result += asending ? " ASC" : " DESC";
        return result;
    }
}