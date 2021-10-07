using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory
{
    public class ConfigurationTablesMemory : IConfigurationTables
    {
        public ConfigurationTablesMemory()
        {
            WorkflowForm = new WorkflowFormTableMemory();
            WorkflowVersion = new WorkflowVersionTableMemory();
            WorkflowVersionParameter = new WorkflowVersionParameterTableMemory();
            ActivityVersionParameter = new ActivityVersionParameterTableMemory();
            ActivityForm = new ActivityFormTableMemory();
            ActivityVersion = new ActivityVersionTableMemory();
            Transition = new TransitionTableMemory();
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
