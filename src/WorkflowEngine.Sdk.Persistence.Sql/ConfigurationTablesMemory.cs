using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class ConfigurationTablesSql : IConfigurationTables
    {
        public ConfigurationTablesSql(string connectionString)
        {
            WorkflowForm = new WorkflowFormTableSql(connectionString);
            WorkflowVersion = new WorkflowVersionTableSql(connectionString);
            MethodParameter= new MethodParameterTableSql(connectionString);
            ActivityForm = new ActivityFormTableSql(connectionString);
            ActivityVersion = new ActivityVersionTableSql(connectionString);
            Transition = new TransitionTableSql(connectionString);
        }
        /// <inheritdoc />
        public IWorkflowFormTable WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionTable WorkflowVersion { get; }

        /// <inheritdoc />
        public IMethodParameterTable MethodParameter { get; }

        /// <inheritdoc />
        public IActivityFormTable ActivityForm { get; }

        /// <inheritdoc />
        public IActivityVersionTable ActivityVersion { get; }

        /// <inheritdoc />
        public ITransitionTable Transition { get; }
    }
}
