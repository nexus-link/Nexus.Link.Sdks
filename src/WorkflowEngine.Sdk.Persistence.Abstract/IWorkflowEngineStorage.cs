using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Containers;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    /// <summary>
    /// All storage
    /// </summary>
    public interface IWorkflowEngineStorage :
        IDeleteAll
    {
        /// <summary>
        /// Service for storing the state for workflow instances, i.e. <see cref="Sdk.Abstract.State.Entities.WorkflowSummary"/>.
        /// </summary>
        IWorkflowSummaryStore WorkflowSummary { get; }
    }
}