using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    public interface IRuntimeTables
    {
        IWorkflowInstanceTable WorkflowInstance{ get; }
        IActivityInstanceTable ActivityInstance{ get; }
    }
}