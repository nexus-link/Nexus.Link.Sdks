using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Containers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Containers;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory
{
    /// <summary>
    /// All storage
    /// </summary>
    public class WorkflowEngineStorageMemory : IWorkflowEngineStorage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowEngineStorageMemory()
        {
            WorkflowSummary = new WorkflowSummaryStoreMemory();
        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            await WorkflowSummary.DeleteAllAsync(cancellationToken);
        }

        /// <inheritdoc />
        public IWorkflowSummaryStore WorkflowSummary { get; }
    }
}