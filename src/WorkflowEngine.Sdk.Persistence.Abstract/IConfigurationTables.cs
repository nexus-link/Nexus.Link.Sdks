using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    public interface IConfigurationTables
    {
        IWorkflowFormTable WorkflowForm { get; }
        IWorkflowVersionTable WorkflowVersion { get; }
        IMethodParameterTable MethodParameter { get; }
        IActivityFormTable ActivityForm { get; }
        IActivityVersionTable ActivityVersion { get; }
        ITransitionTable Transition { get; }
    }
}