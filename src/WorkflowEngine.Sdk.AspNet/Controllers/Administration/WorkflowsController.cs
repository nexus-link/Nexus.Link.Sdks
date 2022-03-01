using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public abstract class WorkflowsController : ControllerBase, IWorkflowService
    {
        private readonly IWorkflowMgmtCapability _capability;

        /// <summary>
        /// Controller
        /// </summary>
        protected WorkflowsController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("Workflows/{id}")]
        public async Task<Workflow> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var workflow = await _capability.Workflow.ReadAsync(id, cancellationToken);
            return workflow;
        }

        /// <inheritdoc />
        [HttpPost("Workflows/{id}/Cancel")]
        public async Task CancelAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            await _capability.Workflow.CancelAsync(id, cancellationToken);
        }
    }
}
