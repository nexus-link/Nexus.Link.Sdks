using WorkflowEngine.Persistence.Abstract;
using WorkflowEngine.Persistence.Abstract.Tables;
using WorkflowEngine.Persistence.Memory.Tables;

namespace WorkflowEngine.Persistence.Memory
{
    public class ConfigurationTablesMemory : IConfigurationTables
    {
        public ConfigurationTablesMemory()
        {
            WorkflowForm = new WorkflowFormTableMemory();
            WorkflowVersion = new WorkflowVersionTableMemory();
            MethodParameter= new MethodParameterTableMemory();
            ActivityForm = new ActivityFormTableMemory();
            ActivityVersion = new ActivityVersionTableMemory();
            Transition = new TransitionTableMemory();
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
