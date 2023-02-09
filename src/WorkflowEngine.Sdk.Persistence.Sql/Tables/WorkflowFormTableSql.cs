using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class WorkflowFormTableSql : CrudSql<WorkflowFormRecordCreate, WorkflowFormRecord>, IWorkflowFormTable
    {
        public WorkflowFormTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "WorkflowForm",
            CreatedAtColumnName = nameof(WorkflowFormRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(WorkflowFormRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(WorkflowFormRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(WorkflowFormRecord.CapabilityName),
                nameof(WorkflowFormRecord.Title),
            },
            OrderBy = new List<string> { nameof(WorkflowFormRecord.RecordCreatedAt) },
            UpdateCanUseOutput = true
        })
        {
        }

        public Task<WorkflowFormRecord> FindByCapabilityNameAndTitleAsync(string capabilityName, string title,
            CancellationToken cancellationToken = default)
        {
            var search = new WorkflowFormRecordCreate
            {
                CapabilityName = capabilityName,
                Title = title
            };
            return FindUniqueAsync(new SearchDetails<WorkflowFormRecord>(search), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<WorkflowFormRecordOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, CancellationToken cancellationToken = default)
        {
            await using var connection = await Database.NewSqlConnectionAsync(cancellationToken);

            const string dateRestriction = "AND StartedAt BETWEEN @instancesFrom AND @instancesTo";
            var query = "SELECT f.Title, CAST(f.Id AS nvarchar(64)) AS Id, f.CapabilityName, v.Id AS ignored," +
                        $"    (SELECT count(*) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id {dateRestriction}) AS InstanceCount," +
                        $"    (SELECT count(*) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id {dateRestriction} AND State = '{WorkflowStateEnum.Executing}') AS ExecutingCount," +
                        $"    (SELECT count(*) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id {dateRestriction} AND State = '{WorkflowStateEnum.Waiting}') AS WaitingCount," +
                        $"    (SELECT count(*) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id {dateRestriction} AND State = '{WorkflowStateEnum.Success}') AS SucceededCount," +
                        $"    (SELECT count(*) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id {dateRestriction} AND State IN @failedStates) AS ErrorCount" +
                        "  FROM WorkflowForm f" +
                        "  JOIN WorkflowVersion v ON (v.WorkflowFormId = f.Id)" +
                        "  GROUP BY f.Title, f.Id, f.CapabilityName, v.Id" +
                        "  ORDER BY f.Title";
            var result = await connection.QueryAsync(query, new
            {
                instancesFrom,
                instancesTo,
                failedStates = new List<string>
                {
                    WorkflowStateEnum.Halting.ToString(),
                    WorkflowStateEnum.Halted.ToString(),
                    WorkflowStateEnum.Failed.ToString(),
                }
            });

            var forms = new List<WorkflowFormRecordOverview>();
            foreach (var row in result)
            {
                forms.Add(new WorkflowFormRecordOverview
                {
                    Id = row.Id,
                    CapabilityName = row.CapabilityName,
                    Title = row.Title,
                    Overview = new WorkflowFormInstancesOverview
                    {
                        InstancesFrom = instancesFrom,
                        InstancesTo = instancesTo,
                        InstanceCount = row.InstanceCount,
                        ExecutingCount = row.ExecutingCount,
                        WaitingCount = row.WaitingCount,
                        ErrorCount = row.ErrorCount,
                        SucceededCount = row.SucceededCount
                    }
                });
            }
            return forms;
        }
    }
}