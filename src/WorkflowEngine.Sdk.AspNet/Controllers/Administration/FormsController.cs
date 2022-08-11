using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public abstract class FormsController : ControllerBase, IFormService
    {
        private readonly IWorkflowMgmtCapability _capability;

        /// <summary>
        /// Controller
        /// </summary>
        protected FormsController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        [HttpGet("Forms/{id}")]
        public async Task<Form> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return await _capability.Form.ReadAsync(id, cancellationToken);
        }

        [HttpGet("Forms")]
        public async Task<PageEnvelope<Form>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));

            return await _capability.Form.ReadAllWithPagingAsync(offset, limit, cancellationToken);
        }
    }
}
