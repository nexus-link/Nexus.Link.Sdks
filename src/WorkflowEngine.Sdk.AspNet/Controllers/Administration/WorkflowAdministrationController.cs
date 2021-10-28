using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration
{
    /// <inheritdoc cref="IWorkflowAdministrationService" />
    public abstract class WorkflowAdministrationController : ControllerBase, IWorkflowAdministrationService
    {
        private readonly IWorkflowCapability _capability;

        protected WorkflowAdministrationController(IWorkflowCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("Workflows/{workflowInstanceId}")]
        public async Task<Workflow> ReadAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var workflow = await _capability.WorkflowAdministrationService.ReadAsync(workflowInstanceId, cancellationToken);
            return workflow;
        }

        /// <inheritdoc />
        [HttpPost("Workflows/{workflowInstanceId}/Cancel")]
        public async Task CancelWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            await _capability.WorkflowAdministrationService.CancelWorkflowAsync(workflowInstanceId, cancellationToken);
        }


        /// <inheritdoc />
        [HttpPost("Activities/{workflowInstanceId}/Retry")]
        public async Task RetryActivityAsync(string activityInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));

            await _capability.WorkflowAdministrationService.RetryActivityAsync(activityInstanceId, cancellationToken);
        }
    }
}
