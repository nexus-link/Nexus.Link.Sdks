using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Containers;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract
{
    /// <summary>
    /// All storage
    /// </summary>
    public class NoWorkflowEngineStorage : IWorkflowEngineStorage
    {
        /// <inheritdoc />
        public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public IWorkflowSummaryStore WorkflowSummary =>
            throw new FulcrumNotImplementedException("No storage has been injected.");
    }
}