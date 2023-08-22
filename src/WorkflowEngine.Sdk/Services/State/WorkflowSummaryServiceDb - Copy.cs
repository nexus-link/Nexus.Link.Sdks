using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State
{
    /// <inheritdoc />
    public class WorkflowSummaryServiceStorage : IWorkflowSummaryServiceStorage
    {
        private readonly IWorkflowEngineStorage _storage;

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowSummaryServiceStorage(IWorkflowEngineStorage storage)
        {
            _storage = storage;
        }
        /// <inheritdoc />
        public async Task<WorkflowSummary> ReadBlobAsync(string workflowInstanceId, DateTimeOffset instanceStartedAt, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
            var workflowSummary = await _storage.WorkflowSummary.ReadAsync(path, cancellationToken);
            return workflowSummary;
        }

        /// <inheritdoc />
        public async Task WriteBlobAsync(WorkflowSummary workflowSummary, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(workflowSummary, nameof(workflowSummary));
            var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowSummary.Instance.Id);
            await _storage.WorkflowSummary.CreateOrUpdateAsync(path, workflowSummary, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteBlobAsync(string workflowInstanceId, DateTimeOffset instanceStartedAt,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
            await _storage.WorkflowSummary.DeleteAsync(path, cancellationToken);
        }
    }
}
