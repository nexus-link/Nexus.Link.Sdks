using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class RuntimeTablesMemory : IRuntimeTables
    {
        public RuntimeTablesMemory(string connectionString)
        {
            WorkflowInstance = new WorkflowInstanceTableSql(connectionString);
            ActivityInstance = new ActivityInstanceTableSql(connectionString);
        }

        /// <inheritdoc />
        public IWorkflowInstanceTable WorkflowInstance { get; }

        /// <inheritdoc />
        public IActivityInstanceTable ActivityInstance { get; }
    }
}
