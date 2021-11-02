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
    public abstract class WorkflowController : ControllerBase, IWorkflowSummaryService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected WorkflowController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("{workflowInstanceId}")]
        public async Task<WorkflowSummary> ReadAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var workflow = await _capability.WorkflowSummary.ReadAsync(workflowInstanceId, cancellationToken);
            return workflow;
        }
    }
}
