using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    public interface IRuntimeTables : IDeleteAll
    {
        IWorkflowInstanceTable WorkflowInstance{ get; }
        IActivityInstanceTable ActivityInstance{ get; }
        ILogTable Log { get; }
    }
}