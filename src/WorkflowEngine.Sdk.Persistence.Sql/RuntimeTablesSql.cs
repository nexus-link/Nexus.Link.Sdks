using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class RuntimeTablesSql : IRuntimeTables
    {
        public RuntimeTablesSql(IDatabaseOptions options)
        {
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
    }
}
