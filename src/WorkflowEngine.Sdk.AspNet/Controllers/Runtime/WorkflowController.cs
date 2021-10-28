using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Runtime
{
    /// <inheritdoc cref="IWorkflowService" />
    public abstract class WorkflowController : ControllerBase, IWorkflowService
    {
        private readonly IWorkflowCapability _capability;

        protected WorkflowController(IWorkflowCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("{workflowInstanceId}")]
        public async Task<Workflow> ReadAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var workflow = await _capability.Workflow.ReadAsync(workflowInstanceId, cancellationToken);
            return workflow;
        }
    }
}
