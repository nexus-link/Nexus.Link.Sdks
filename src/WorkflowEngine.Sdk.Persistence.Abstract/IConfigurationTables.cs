using WorkflowEngine.Persistence.Abstract.Tables;

namespace WorkflowEngine.Persistence.Abstract
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