using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public abstract class WorkflowsController : ControllerBase, IWorkflowService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected WorkflowsController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("Workflows/{workflowInstanceId}")]
        public async Task<Workflow> ReadAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var workflow = await _capability.Workflow.ReadAsync(workflowInstanceId, cancellationToken);
            return workflow;
        }

        /// <inheritdoc />
        [HttpPost("Workflows/{workflowInstanceId}/Cancel")]
        public async Task CancelWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            await _capability.Workflow.CancelWorkflowAsync(workflowInstanceId, cancellationToken);
        }


        /// <inheritdoc />
        [HttpPost("Activities/{activityInstanceId}/Retry")]
        public async Task RetryActivityAsync(string activityInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));

            await _capability.Workflow.RetryActivityAsync(activityInstanceId, cancellationToken);
        }
    }
}
