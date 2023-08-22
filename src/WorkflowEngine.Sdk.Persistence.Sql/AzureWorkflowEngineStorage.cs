using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Containers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Containers;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    /// <summary>
    /// All storage
    /// </summary>
    public class AzureWorkflowEngineStorage : IWorkflowEngineStorage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AzureWorkflowEngineStorage(string connectionString)
        {
            WorkflowSummary = new WorkflowSummaryStore(connectionString, "workflow-summary");
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