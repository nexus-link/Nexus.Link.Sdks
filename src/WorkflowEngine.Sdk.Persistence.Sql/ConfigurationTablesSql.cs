using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class ConfigurationTablesSql : IConfigurationTables
    {
        public ConfigurationTablesSql(IDatabaseOptions options)
        {
            options.DistributedLockTable = new DistributedLockTable(options);
            // This is the error number we throw in our trigger constraints, see "patch08 - triggers.sql"
            options.TriggerConstraintSqlExceptionErrorNumber = 100550;
            WorkflowForm = new WorkflowFormTableSql(options);
            WorkflowVersion = new WorkflowVersionTableSql(options);
            ActivityForm = new ActivityFormTableSql(options);
            ActivityVersion = new ActivityVersionTableSql(options);
        }
        /// <inheritdoc />
        public IWorkflowFormTable WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionTable WorkflowVersion { get; }

        /// <inheritdoc />
        public IActivityFormTable ActivityForm { get; }

        /// <inheritdoc />
        public IActivityVersionTable ActivityVersion { get; }

        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            // ActivityVersion
            var activityVersion = ActivityVersion as CrudSql<ActivityVersionRecordCreate, ActivityVersionRecord>;
            FulcrumAssert.IsNotNull(activityVersion, CodeLocation.AsString());
            await activityVersion!.DeleteAllAsync(cancellationToken);

            // ActivityForm
            var activityForm = ActivityForm as CrudSql<ActivityFormRecordCreate, ActivityFormRecord>;
            FulcrumAssert.IsNotNull(activityVersion, CodeLocation.AsString());
            await activityForm!.DeleteAllAsync(cancellationToken);

            // WorkflowVersion
            var workflowVersion = WorkflowVersion as CrudSql<WorkflowVersionRecordCreate, WorkflowVersionRecord>;
            FulcrumAssert.IsNotNull(workflowVersion, CodeLocation.AsString());
            await workflowVersion!.DeleteAllAsync(cancellationToken);

            // WorkflowForm
            var workflowForm = WorkflowForm as CrudSql<WorkflowFormRecordCreate, WorkflowFormRecord>;
            FulcrumAssert.IsNotNull(workflowVersion, CodeLocation.AsString());
            await workflowForm!.DeleteAllAsync(cancellationToken);
        }
    }
}
