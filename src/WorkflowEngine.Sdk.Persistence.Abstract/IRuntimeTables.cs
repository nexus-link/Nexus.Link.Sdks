using WorkflowEngine.Persistence.Abstract.Tables;

namespace WorkflowEngine.Persistence.Abstract
{
    public interface IRuntimeTables
    {
        IWorkflowInstanceTable WorkflowInstance{ get; }
        IActivityInstanceTable ActivityInstance{ get; }
    }
}