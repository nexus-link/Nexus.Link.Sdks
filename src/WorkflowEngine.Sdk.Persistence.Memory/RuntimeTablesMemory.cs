using WorkflowEngine.Persistence.Abstract;
using WorkflowEngine.Persistence.Abstract.Tables;
using WorkflowEngine.Persistence.Memory.Tables;

namespace WorkflowEngine.Persistence.Memory
{
    public class RuntimeTablesMemory : IRuntimeTables
    {
        public RuntimeTablesMemory()
        {
            WorkflowInstance = new WorkflowInstanceTableMemory();
            ActivityInstance = new ActivityInstanceTableMemory();
        }

        /// <inheritdoc />
        public IWorkflowInstanceTable WorkflowInstance { get; }

        /// <inheritdoc />
        public IActivityInstanceTable ActivityInstance { get; }
    }
}
