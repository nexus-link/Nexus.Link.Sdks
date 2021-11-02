using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.State
{
    /// <inheritdoc cref="IWorkflowSummaryService" />
    public abstract class WorkflowSummariesController : ControllerBase, IWorkflowSummaryService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected WorkflowSummariesController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("Forms/{formId}/Versions/{majorVersion}/Instances/{instanceId}")]
        public async Task<WorkflowSummary> GetSummaryAsync(string formId, int majorVersion, string instanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
            ServiceContract.RequireGreaterThanOrEqualTo(0, majorVersion, nameof(majorVersion));
            ServiceContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));

            var workflowSummary = await _capability.WorkflowSummary.GetSummaryAsync(formId, majorVersion, instanceId, cancellationToken);
            return workflowSummary;
        }

        /// <inheritdoc />
        [HttpGet("Instances/{instanceId}")]
        public async Task<WorkflowSummary> GetSummaryAsync(string instanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));

            var workflowSummary = await _capability.WorkflowSummary.GetSummaryAsync(instanceId, cancellationToken);
            return workflowSummary;
        }
    }
}
