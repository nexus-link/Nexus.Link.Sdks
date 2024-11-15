using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
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
        public async Task<IList<WorkflowFormRecordOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, FormOverviewIncludeFilter filter, CancellationToken cancellationToken = default)
        {
            await using var connection = await Database.NewSqlConnectionAsync(cancellationToken);

            const string dateRestriction = "AND RecordCreatedAt BETWEEN @instancesFrom AND @instancesTo";
            const string versionRestriction = @"( SELECT wf.Id , wf.Majorversion , wf.MinorVersion, wf.WorkflowFormId
                                                 FROM WorkflowVersion wf, (SELECT WorkflowFormId, max(Majorversion) v from WorkflowVersion GROUP by WorkflowFormId ) g
                                                 WHERE wf.WorkflowFormId = g.WorkflowFormId and wf.Majorversion = g.v )";
            const string noVersionRestriction = "WorkflowVersion";
            const string instanceCountRestriction = " HAVING InstanceCount > 0 ";

            var query = $@"SELECT Title, Id, CapabilityName, Version, InstanceCount, ExecutingCount, WaitingCount, SucceededCount, ErrorCount, ignored 
                             FROM( SELECT f.Title, CAST(f.Id AS nvarchar(64)) AS Id, f.CapabilityName, v.Id AS ignored,
                                        (SELECT count(Id) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id { dateRestriction}) AS InstanceCount,
                                        (SELECT count(Id) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id { dateRestriction} AND State = '{WorkflowStateEnum.Executing}') AS ExecutingCount,
                                        (SELECT count(Id) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id { dateRestriction} AND State = '{WorkflowStateEnum.Waiting}') AS WaitingCount,
                                        (SELECT count(Id) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id { dateRestriction} AND State = '{WorkflowStateEnum.Success}') AS SucceededCount,
                                        (SELECT count(Id) FROM WorkflowInstance WHERE WorkflowVersionId = v.Id { dateRestriction} AND State IN @failedStates) AS ErrorCount,
                                        CONCAT(v.MajorVersion, '.', v.MinorVersion) as Version
                                  FROM WorkflowForm f
                                  JOIN {((filter == FormOverviewIncludeFilter.LatestVersion) ? versionRestriction : noVersionRestriction )} v ON(v.WorkflowFormId = f.Id) ) A
                                  GROUP BY Title, Id, CapabilityName, Version, InstanceCount, ExecutingCount, WaitingCount, SucceededCount, ErrorCount, ignored
                                  {((filter == FormOverviewIncludeFilter.ExcludeZeroInstanceCount) ? instanceCountRestriction : string.Empty)}
                                  ORDER BY Title";
        
            var result = await connection.QueryAsync(query.ToString(), new
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
                    Version = row.Version,
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