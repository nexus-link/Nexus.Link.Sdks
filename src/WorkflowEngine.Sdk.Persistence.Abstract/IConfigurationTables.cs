using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    public interface IConfigurationTables : IDeleteAll
    {
        IWorkflowFormTable WorkflowForm { get; }
        IWorkflowVersionTable WorkflowVersion { get; }
        IActivityFormTable ActivityForm { get; }
        IActivityVersionTable ActivityVersion { get; }
    }
}