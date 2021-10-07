using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    public interface IConfigurationTables
    {
        IWorkflowFormTable WorkflowForm { get; }
        IWorkflowVersionTable WorkflowVersion { get; }
        IWorkflowVersionParameterTable WorkflowVersionParameter { get; }
        IActivityVersionParameterTable ActivityVersionParameter { get; }
        IActivityFormTable ActivityForm { get; }
        IActivityVersionTable ActivityVersion { get; }
        ITransitionTable Transition { get; }
    }
}