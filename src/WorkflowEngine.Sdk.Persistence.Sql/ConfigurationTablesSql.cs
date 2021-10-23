using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class ConfigurationTablesSql : IConfigurationTables
    {
        public ConfigurationTablesSql(IDatabaseOptions options)
        {
            WorkflowForm = new WorkflowFormTableSql(options);
            WorkflowVersion = new WorkflowVersionTableSql(options);
            WorkflowVersionParameter = new WorkflowVersionParameterTableSql(options);
            ActivityVersionParameter = new ActivityVersionParameterTableSql(options);
            ActivityForm = new ActivityFormTableSql(options);
            ActivityVersion = new ActivityVersionTableSql(options);
            Transition = new TransitionTableSql(options);
        }
        /// <inheritdoc />
        public IWorkflowFormTable WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionTable WorkflowVersion { get; }

        /// <inheritdoc />
        public IWorkflowVersionParameterTable WorkflowVersionParameter { get; }

        /// <inheritdoc />
        public IActivityVersionParameterTable ActivityVersionParameter { get; }

        /// <inheritdoc />
        public IActivityFormTable ActivityForm { get; }

        /// <inheritdoc />
        public IActivityVersionTable ActivityVersion { get; }

        /// <inheritdoc />
        public ITransitionTable Transition { get; }
    }
}
