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
    public class RuntimeTablesSql : IRuntimeTables
    {
        public RuntimeTablesSql(IDatabaseOptions options)
        {
            options.DistributedLockTable = new DistributedLockTable(options);
            // This is the error number we throw in our trigger constraints, see "patch08 - triggers.sql"
            options.TriggerConstraintSqlExceptionErrorNumber = 100550;
            WorkflowInstance = new WorkflowInstanceTableSql(options);
            ActivityInstance = new ActivityInstanceTableSql(options);
            Log = new LogTableSql(options);
        }

        /// <inheritdoc />
        public IWorkflowInstanceTable WorkflowInstance { get; }

        /// <inheritdoc />
        public IActivityInstanceTable ActivityInstance { get; }

        /// <inheritdoc />
        public ILogTable Log { get; }

        /// <inheritdoc />
        public async Task DeleteAllAsync()
        {
            // ActivityInstance
            var activityInstance = ActivityInstance as CrudSql<ActivityInstanceRecordCreate, ActivityInstanceRecord>;
            FulcrumAssert.IsNotNull(activityInstance, CodeLocation.AsString());
            await activityInstance!.DeleteAllAsync();
            
            // WorkflowInstance
            var workflowInstance = WorkflowInstance as CrudSql<WorkflowInstanceRecordCreate, WorkflowInstanceRecord>;
            FulcrumAssert.IsNotNull(workflowInstance, CodeLocation.AsString());
            await workflowInstance!.DeleteAllAsync();
        }
    }
}
